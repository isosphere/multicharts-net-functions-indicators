using System;
using System.Drawing;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator {
	[SameAsSymbol(true)]
	public class Volatility_Levels : IndicatorObject {
		[Input] public int length { get; set; }
		[Input] public double numdevs { get; set; }
		[Input] public bool enable_depth { get; set; }

		public Volatility_Levels(object _ctx): base(_ctx) {
			numdevs = 1;
			length = 28;
			enable_depth = true;
		}

		private VariableSeries<Double> m_lowerband;
		private VariableSeries<Double> m_upperband;

		private IPlotObject Plot1, Plot2, Plot3, Plot4, Plot5;

		private VolatilityStdDev_ClosedOnly volstddev;

		protected override void Create() {
			m_lowerband = new VariableSeries<Double>(this);
			m_upperband = new VariableSeries<Double>(this);
			volstddev = new VolatilityStdDev_ClosedOnly(this);
			
			Plot1 = AddPlot(new PlotAttributes("UpperBand", 0, Color.Red, Color.Empty, 1, 0, true));
			Plot2 = AddPlot(new PlotAttributes("LowerBand", 0, Color.Green, Color.Empty, 1, 0, true));
			
			if (enable_depth) {
				Plot3 = AddPlot(new PlotAttributes("UpperDepth", 0, Color.Red, Color.Empty, 1, 0, true));
				Plot4 = AddPlot(new PlotAttributes("LowerDepth", 0, Color.Red, Color.Empty, 1, 0, true));
			}
		}	

		protected override void StartCalc() {			
			volstddev.numdays = length;
		}

		protected override void StopCalc() {
		}

		protected override void CalcBar() {
			if (Bars.LastBarInSession) {
				m_upperband.Value = (Bars.Close[0] * ( 1 + (numdevs * volstddev[0]/Math.Sqrt(252))));
				m_lowerband.Value = (Bars.Close[0] * ( 1 - (numdevs * volstddev[0]/Math.Sqrt(252))));
			} else {	
				m_upperband.Value = (Bars.Close[1] * ( 1 + (numdevs * volstddev[0]/Math.Sqrt(252))));
				m_lowerband.Value = (Bars.Close[1] * ( 1 - (numdevs * volstddev[0]/Math.Sqrt(252))));
			}
			
			Plot1.Set(0, m_upperband.Value);
			Plot2.Set(0, m_lowerband.Value);     
			
			if (enable_depth) {
				Plot3.Set(0, (Bars.Close[0] - m_upperband.Value) / (m_upperband.Value - m_lowerband.Value));
				Plot4.Set(0, (m_lowerband.Value - Bars.Close[0]) / (m_upperband.Value - m_lowerband.Value));
			}
		}
	}
}
