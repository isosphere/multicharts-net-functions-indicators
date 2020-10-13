using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator{
	public class Z_Score_Previous : IndicatorObject {
		public Z_Score_Previous(object _ctx):base(_ctx){}
		private IPlotObject plot1;
		private ZScore zscore;
		
		[Input] public int length { get; set; }
		
		protected override void Create() {
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Yellow));
			zscore = new ZScore(this);
			length = 28;
		}
		protected override void StartCalc() {
			zscore.length = length;
		}
		protected override void CalcBar(){
			plot1.Set(zscore[1]);
		}
	}
}