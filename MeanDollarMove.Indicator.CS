using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator {
	public class MeanDollarMove : IndicatorObject {
		public MeanDollarMove(object _ctx):base(_ctx){}
		private IPlotObject plot1;
		
		[Input] public int length { get; set; }
		[Input] public int position { get; set; }
		private MeanAbsoluteDifference mean_absolute_difference;
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Red));
			mean_absolute_difference = new MeanAbsoluteDifference(this);
			length = 28;	
			position = 1;
		}
		protected override void StartCalc() {
			mean_absolute_difference.length = length;
			mean_absolute_difference.target = Bars.Close;
		}
		protected override void CalcBar(){
			// indicator logic 
			plot1.Set(Bars.Info.BigPointValue*mean_absolute_difference[0]*position);
		}
	}
}