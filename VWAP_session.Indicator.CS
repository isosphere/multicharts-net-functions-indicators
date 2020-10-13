using System;
using System.Drawing;
using PowerLanguage.Function;

// Matthew Scheffel, matt@dataheck.com
// Code used "vwapResettable" as a base

namespace PowerLanguage.Indicator {
    [SameAsSymbol(true)]
    public class VWAP_session : IndicatorObject {
        private vwapSession m_vwapresettable1;

        private VariableSeries<Double> m_price;
        private VariableSeries<Double> m_it;
        private VariableObject<Color> m_color;

        private IPlotObject Plot1;

        [Input] public Color upcolor { get; set; }
        [Input] public Color dncolor { get; set; }
		
		public VWAP_session(object ctx):base(ctx) {
            dncolor = Color.Magenta;
            upcolor = Color.Cyan;
        }

        protected override void Create() {
            m_vwapresettable1 = new vwapSession(this);
            m_price = new VariableSeries<Double>(this);
            m_it = new VariableSeries<Double>(this);
            m_color = new VariableObject<Color>(this);
            Plot1 = AddPlot(new PlotAttributes("vwap_reset", EPlotShapes.Point, Color.Blue, Color.Empty, 0, 0, true));
        }

        protected override void StartCalc() {
            m_vwapresettable1.price = m_price;
            m_color.DefaultValue = Color.Yellow;
        }

        protected override void CalcBar() {
            m_price.Value = Bars.AvgPrice(0);			
            m_it.Value = m_vwapresettable1[0];
			
            Plot1.Set(0, m_it.Value);
			
            if (PublicFunctions.DoubleGreater(m_it.Value, m_it[1])){
                m_color.Value = upcolor;
            } else {
                m_color.Value = dncolor;
            }
			
            Plot1.Colors[0] = m_color.Value;
        }
    }
}