using System;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class LinearRegressionReversion : SignalObject {
		[Input] public int length { get; set; }
		[Input] public double trigger { get; set; }
		[Input] public bool contrarian { get; set; }
		
		public LinearRegressionReversion(object _ctx):base(_ctx) {
			length = 28;
			trigger = 1.5;
			contrarian = true;
		}
		
		private IOrderMarket long_order, short_order, close_long_order, close_short_order;
		private LinearRegValue current_regression;
		private VariableSeries <double>	deviation;
		
		protected override void Create() {
			long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Buy));
			short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.SellShort));
			close_long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Sell));
			close_short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.BuyToCover));
			
			current_regression = new LinearRegValue(this);
			deviation = new VariableSeries<double>(this);
		}
		
		protected override void StartCalc() {
            current_regression.price = Bars.Close;
            current_regression.length = length;
            current_regression.tgtbar = 0;
		}
		
		protected override void CalcBar() {			
			deviation.Value = Bars.Close[0] - current_regression[0];
					
			double standard_deviation = deviation.StandardDeviationCustom(length, 1);
			
			if (standard_deviation > 0.01) {
				double z_score = deviation.Value / standard_deviation;

				bool close_long, close_short, open_long, open_short;
				
				if (contrarian) {
					open_short = z_score > trigger;
					open_long = z_score < -1*trigger;
					
					close_long = z_score > 0;
					close_short = z_score < 0;
				} else {
					open_long = z_score > trigger;
					open_short = z_score < -1*trigger;
					
					close_short = z_score > 0;
					close_long = z_score < 0;					
				}
				
				if (StrategyInfo.MarketPosition == 0) {
					if (open_long) long_order.Send();
					if (open_short) short_order.Send();
				}
				else if (StrategyInfo.MarketPosition < 0) {
					if (close_short) close_short_order.Send();
				}
				else if (StrategyInfo.MarketPosition > 0) {
					if (close_long) close_long_order.Send();
				}				
			}
		}
	}
}