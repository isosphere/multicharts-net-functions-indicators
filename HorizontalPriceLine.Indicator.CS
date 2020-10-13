using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

// Original code: 'MaRCHeW'
// Modified by Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator {
    [SameAsSymbol(true)]
	public class HorizontalPriceLine : IndicatorObject {
        private VariableObject<ITrendLineObject> lineVal;

        [Input] public Color line_color { get; set; }
		[Input] public ETLStyle line_style { get; set; }
		
		public HorizontalPriceLine(object ctx):base(ctx) {
			line_color = Color.LightGray;
			line_style = ETLStyle.ToolSolid;
		}
		
		protected override void Create() {
            lineVal = new VariableObject<ITrendLineObject>(this);
		}

		protected override void StartCalc() {
            lineVal.DefaultValue = null;
		}

		protected override void CalcBar() {
			double current_level = Bars.Close[0];
			ChartPoint start = new ChartPoint(Bars.Time[1], current_level);
			ChartPoint end = new ChartPoint(Bars.Time[0], current_level);
			
            if (lineVal[0] != null && lineVal[0].Exist) {
                lineVal.Value.Begin = start;
                lineVal.Value.End = end;
            } else {
                lineVal.Value = DrwTrendLine.Create(start, end);
                lineVal.Value.ExtLeft = true;
                lineVal.Value.ExtRight = true;
                lineVal.Value.Color = line_color;
                lineVal.Value.Size = 0;
                lineVal.Value.Style = line_style;
            }
            
		}
	}
}