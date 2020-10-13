using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

/*
SumSq = 0 
MaxValue = 0 
for T = 1 to NumOfPeriods do 
	if Value[T] > MaxValue then MaxValue = Value[T] 
	else SumSq = SumSq + sqr(100 * ((Value[T] / MaxValue) - 1))
UI = sqrt(SumSq / NumOfPeriods)
UPI = (Total Return - Risk Free Return) / UI
*/

namespace PowerLanguage.Strategy {
	public class OptimizeByUlcerIndex : SignalObject {
		public OptimizeByUlcerIndex(object _ctx):base(_ctx){}
		
		private double sum_of_squares;
		private double max_value;
		
		[Input] public double trade_penality_threshhold { get; set; }

		protected override void Create() {
			trade_penality_threshhold = 0.10;
		}
		
		protected override void StartCalc() {
			sum_of_squares = 0.0;
			max_value = this.InitialCapital;
		}
		
		protected override void CalcBar() {
			double activity_score = Math.Min((double) this.TotalTrades/(Bars.FullSymbolData.Count), trade_penality_threshhold)/trade_penality_threshhold;
			double equity_value = this.InitialCapital + StrategyInfo.OpenEquity + StrategyInfo.ClosedEquity;

			if (equity_value > max_value) {
				max_value = equity_value;
			} else {
				sum_of_squares += Math.Pow(100 * ((equity_value / max_value) - 1), 2);
			}
			
			if (Bars.LastBarOnChart) {
				double total_return, ulcer_index;			
				total_return = this.NetProfit / this.InitialCapital;
				ulcer_index = Math.Sqrt(sum_of_squares / Bars.FullSymbolData.Count);
				//Output.WriteLine("return = {0}, ulcer_index = {1}", total_return, ulcer_index);
				if (ulcer_index != 0) {
					CustomFitnessValue = activity_score*total_return / ulcer_index;
				}
			}
		}
	}
}