using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Matthew Scheffel
// matt@dataheck.com

namespace PowerLanguage.Indicator {
	public class Mean_Absolute_Difference : IndicatorObject {
		public Mean_Absolute_Difference(object _ctx):base(_ctx){}
		private IPlotObject plot1;
		
		private MeanAbsoluteDifference mean_absolute_difference;
		
		[Input] public int length { get; set; }
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Red));
			mean_absolute_difference = new MeanAbsoluteDifference(this);
			length = 28;
		}
		protected override void StartCalc() {
			// assign inputs 
			mean_absolute_difference.length = length;
			mean_absolute_difference.target = Bars.Close;
		}
		protected override void CalcBar(){
			// indicator logic 
			plot1.Set(mean_absolute_difference[0]);
		}
	}
}