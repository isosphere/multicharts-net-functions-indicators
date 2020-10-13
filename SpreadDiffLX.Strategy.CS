using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class SpreadDiffLX : SignalObject {
		public SpreadDiffLX(object _ctx):base(_ctx){}
		private IOrderMarket exit_order;
		
		private Spread_Diff_Corrective_Function spread_diff_func;

        [Input] public double multiplier { get; set; }
		
        [Input] public double spread_bias { get; set; }
		[Input] public double static_confidence { get; set; }
				
		[Input] public bool log_prices { get; set; }
		[Input] public bool dynamic_bias { get; set; }
		[Input] public bool dynamic_multiplier { get; set; }
		[Input] public int dynamic_lookback { get; set; }		
		
		protected override void Create() {
			// create variable objects, function objects, order objects etc.
			spread_diff_func = new Function.Spread_Diff_Corrective_Function(this);
			exit_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Sell));
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
		}
		
		protected override void CalcBar(){
			// strategy logic
			spread_diff_func.Call(); // force calculation
			
			// data_1 = series 1
			// data_2 = series 2
			// data_3 = series 1 bid
			// data_4 = series 1 ask
			
			// bid is more than the the highest we think is reasonable - close long

			if (BarsOfData(3).Close[0] > spread_diff_func.ErrorHigh) {
				exit_order.Send();
			}
		}
	}
}