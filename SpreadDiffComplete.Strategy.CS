using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class SpreadDiffComplete : SignalObject {
		public SpreadDiffComplete(object _ctx):base(_ctx){}
		
		private IOrderMarket long_order, close_long_order, short_order, close_short_order;
		
		private Spread_Diff_Corrective_Function spread_diff_func;
		
		public enum price_signal_enum {
			Bid_Ask = 0,
			Trade = 1
		}

        [Input] public double multiplier { get; set; }
		
        [Input] public double spread_bias { get; set; }
		[Input] public double static_confidence { get; set; }
			
		[Input] public bool log_prices { get; set; }
		[Input] public bool dynamic_bias { get; set; }
		[Input] public price_signal_enum price_signal { get; set; }
		[Input] public bool dynamic_multiplier { get; set; }
		[Input] public bool liquidate_at_midpoint { get; set; }
		[Input] public int dynamic_lookback { get; set; }
		[Input] public bool first_difference { get; set; }
		
		[Input] public bool enable_long { get; set; }
		[Input] public bool enable_short { get; set; }		
		
		protected override void Create() {
			// create variable objects, function objects, order objects etc.
			spread_diff_func = new Function.Spread_Diff_Corrective_Function(this);
			
			long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Buy));
			close_long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Sell));
			short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.SellShort));
			close_short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.BuyToCover));
			
			enable_long = true;
			enable_short = true;
			first_difference = false;
			liquidate_at_midpoint = true;
		}
		
		protected override void StartCalc() {
			spread_diff_func.fixed_bias = spread_bias;
			spread_diff_func.fixed_multiplier = multiplier;	
			
			spread_diff_func.dynamic_bias = dynamic_bias;
			spread_diff_func.dynamic_multiplier = dynamic_multiplier;
			spread_diff_func.dynamic_lookback = dynamic_lookback;
			
			spread_diff_func.static_confidence = static_confidence;
			spread_diff_func.log_prices = log_prices;
			
			spread_diff_func.series_a = BarsOfData(1).Close;
			spread_diff_func.series_b = BarsOfData(2).Close;
			
			spread_diff_func.first_difference = first_difference;
		}
		
		protected override void CalcBar(){
			// strategy logic
			spread_diff_func.Call(); // force calculation
			
			// data_1 = series 1
			// data_2 = series 2
			// data_3 = series 1 bid
			// data_4 = series 1 ask
					
			double ask, bid;
			
			if (price_signal == price_signal_enum.Bid_Ask) {
				bid = BarsOfData(3).Close[0];
				ask = BarsOfData(4).Close[0];
			} else {
				bid = BarsOfData(1).Close[0];
				ask = BarsOfData(1).Close[0];
			}
			
			// lift the ask
			if (ask < spread_diff_func.ErrorLow) {
				if (this.StrategyInfo.MarketPosition >= 0 && enable_long) {
					long_order.Send();
				} else if (!liquidate_at_midpoint && this.StrategyInfo.MarketPosition < 0) {
					close_short_order.Send();
				}
			}
			
			if (liquidate_at_midpoint && this.StrategyInfo.MarketPosition < 0 && ask <= spread_diff_func.spread_value) {
				close_short_order.Send();
			}
			
			// hit the bid
			else if (bid > spread_diff_func.ErrorHigh) {
				if (!liquidate_at_midpoint && this.StrategyInfo.MarketPosition > 0) {
					close_long_order.Send();
				} else if (this.StrategyInfo.MarketPosition <= 0 && enable_short) {
					short_order.Send();
				} 
			}
			
			if (liquidate_at_midpoint && this.StrategyInfo.MarketPosition > 0 && bid >= spread_diff_func.spread_value) {
				close_long_order.Send();
			}
		}
	}
}