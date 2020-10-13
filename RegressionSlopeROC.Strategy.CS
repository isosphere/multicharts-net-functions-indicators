using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	[IOGMode(IOGMode.Enabled)]
	public class RegressionSlopeROC : SignalObject {
		public RegressionSlopeROC(object _ctx):base(_ctx ){}
		
		private IOrderMarket long_order, short_order, close_long_order, close_short_order;
		private LinearRegValue current_regression, past_regression;
		private StandardDeviation stdev;
		private VariableSeries<double> slope_roc;
		
		private bool trade_enabled; 
		
		[Input] public int regression_length { get; set; }
		[Input] public int zscore_multiple { get; set; }
		[Input] public double bias_reset_zscore { get; set; } // how deep of a zero-cross changes our bias?
		[Input] public double take_profit_zscore { get; set; } // what z-score of abs roc will close our position
		
		[Input] public bool enable_long { get; set; }
		[Input] public bool enable_short { get; set; }
		
		protected override void Create() {
            current_regression = new LinearRegValue(this);
            past_regression = new LinearRegValue(this);
			stdev = new StandardDeviation(this);
			slope_roc = new VariableSeries<double>(this, regression_length);
			
			long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Buy));
			short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.SellShort));
			close_long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Sell));
			close_short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.BuyToCover));

			enable_long = true;
			enable_short = true;
			
			regression_length = 250;
			bias_reset_zscore = 0.25;
			take_profit_zscore = 0.85;
			zscore_multiple = 10;
			
			trade_enabled = true;
		}
		
		protected override void StartCalc() {
            current_regression.price = Bars.Close;
            current_regression.length = regression_length;
            current_regression.tgtbar = 0;
			
            past_regression.price = Bars.Close;
            past_regression.length = regression_length;
            past_regression.tgtbar = regression_length;
			
			stdev.values = slope_roc;
			stdev.length = zscore_multiple * regression_length;
		}
		
		protected override void CalcBar() {
			slope_roc.Value = (current_regression[0] - past_regression[0]) - (current_regression[1] - past_regression[1]);
			stdev.Call();
			
			//Output.WriteLine("zscore={0}, slope_roc.Value={1}", stdev[0], slope_roc.Value);
			
			bool close_long, close_short, open_long, open_short;
			
			close_long = (slope_roc.Value > take_profit_zscore) || (slope_roc.Value < -bias_reset_zscore);
			close_short = (slope_roc.Value < -take_profit_zscore) || (slope_roc.Value > bias_reset_zscore);
			
			if (!trade_enabled && ((slope_roc[0] > 0.0 && slope_roc[1] < 0.0) || (slope_roc[0] < 0.0 && slope_roc[1] > 0.0))) {
				trade_enabled = true;
			}
			
			open_long = (slope_roc.Value > bias_reset_zscore) && trade_enabled;
			open_short = (slope_roc.Value < -bias_reset_zscore) && trade_enabled;			
			
			if (StrategyInfo.MarketPosition == 0) {
				if (open_long) long_order.Send();
				if (open_short) short_order.Send();
			}
			else if (StrategyInfo.MarketPosition < 0) {
				if (close_short) {
					close_short_order.Send();
					trade_enabled = false;
				}
			}
			else if (StrategyInfo.MarketPosition > 0) {
				if (close_long) {
					close_long_order.Send();
					trade_enabled = false;
				}
			}
		}
	}
}