using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator{
	public class Volume_Z_Score : IndicatorObject {
		public Volume_Z_Score(object _ctx):base(_ctx){}
		private IPlotObject plot1;
		private ZScoreVolume zscore;
		
		[Input] public int length { get; set; }
		[Input] public bool skip_current { get; set; }
		
		protected override void Create() {
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Histogram, Color.Yellow));
			zscore = new ZScoreVolume(this);
			length = 28;
			skip_current = true;
		}
		protected override void StartCalc() {
			zscore.length = length;
			zscore.skip_current = skip_current;
		}
		protected override void CalcBar(){
			plot1.Set(zscore[0]);
		}
	}
}