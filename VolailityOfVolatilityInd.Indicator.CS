using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator{
	public class VolailityOfVolatilityInd : IndicatorObject {
		private static readonly double m_k = Math.Sqrt(252);
		[Input] public int length { get; set; }
		
		public VolailityOfVolatilityInd(object _ctx):base(_ctx){
			length = 28;
		}
		
		private IPlotObject plot1;
		private VolatilityOfVolatilityStdDev volofvol;
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Red));
			volofvol = new VolatilityOfVolatilityStdDev(this);
		}
		
		protected override void StartCalc() {
			volofvol.numdays = length;
			volofvol.price = Bars.Close;
		}
		
		protected override void CalcBar() {
			// indicator logic 
			plot1.Set(m_k*volofvol[0]);
		}
	}
}