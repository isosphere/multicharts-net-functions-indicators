using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator{
	public class FirstDifferenceInd : IndicatorObject {
		public FirstDifferenceInd(object _ctx):base(_ctx){}
		private IPlotObject plot1;
		
		private FirstDifference difference;
			
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Red));
			difference = new Function.FirstDifference(this);
		}
		protected override void StartCalc() {
			// assign inputs 
			difference.Price = BarsOfData(1).Close;
		}
		protected override void CalcBar(){
			// indicator logic 
			plot1.Set(difference[0]);
		}
	}
}