using System;
using System.Drawing;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator {
	[SameAsSymbol(true)]
	public class Mean_Absolute_Difference_Bands : IndicatorObject, IChartCustomDrawer {
		[Input] public int length { get; set; }
		[Input] public bool delta { get; set; }
		[Input] public double multiplier { get; set; }
		[Input] public Color fillcolor { get; set; }
		[Input] public byte transparency {
			get {
				byte res = Convert.ToByte((float)m_intensity / 255.0 * 100.0);
				return res;
			} 
			set {
				try {
					m_intensity = Convert.ToByte(255.0 / 100.0 * (float)value);
				} catch (Exception) {
					transparency = 100;
				}
			}
		}
		
		public Mean_Absolute_Difference_Bands(object _ctx): base(_ctx) {
			length = 28;
			multiplier = 1.00;

			fillcolor = Color.FromArgb(128, 128, 255);
			transparency = 85;
		}

		private VariableSeries<Double> m_lowerband;
		private VariableSeries<Double> m_upperband;

		private IPlotObject Plot1;
		private IPlotObject Plot2;

		private AreaValue m_IndicatorArea;
		
		private MeanAbsoluteDifference mean_absolute_difference_fn;
		private MeanAbsoluteDifference_Delta mean_absolute_difference_delta_fn;

		private bool isCSDRegistred;
    	private byte m_intensity; // 0-255

		protected override void Create() {
			m_lowerband = new VariableSeries<Double>(this);
			m_upperband = new VariableSeries<Double>(this);
			
			mean_absolute_difference_fn = new MeanAbsoluteDifference(this);
			mean_absolute_difference_delta_fn = new MeanAbsoluteDifference_Delta(this);			
			
			Plot1 = AddPlot(new PlotAttributes("UpperBand", 0, Color.Green, Color.Empty, 1, 0, true));
			Plot2 = AddPlot(new PlotAttributes("LowerBand", 0, Color.Red, Color.Empty, 1, 0, true));

			m_IndicatorArea = new AreaValue();
			isCSDRegistred = false;
			delta = false;
		}	

		protected override void StartCalc() {
			if (!isCSDRegistred) {
				ChartCustomDraw.Register(this);
				isCSDRegistred = true;
			}
			
			mean_absolute_difference_fn.length = length;
			mean_absolute_difference_fn.target = Bars.Close;			
			
			mean_absolute_difference_delta_fn.length = length;
			mean_absolute_difference_delta_fn.target = Bars.Close;
		}

		protected override void StopCalc() {
			if (isCSDRegistred) {
				ChartCustomDraw.Unregister(this);
				isCSDRegistred = false;
			}
		}

		protected override void CalcBar() {
			if (delta) {
				m_upperband.Value = Bars.Close[1] + multiplier*mean_absolute_difference_delta_fn[1];
				m_lowerband.Value = Bars.Close[1] - multiplier*mean_absolute_difference_delta_fn[1];
			} else {
				m_upperband.Value = Bars.Close[1] + multiplier*mean_absolute_difference_fn[1];
				m_lowerband.Value = Bars.Close[1] - multiplier*mean_absolute_difference_fn[1];				
			}
			
			Plot1.Set(0, m_upperband.Value);
			Plot2.Set(0, m_lowerband.Value);

			m_IndicatorArea.SetValue("UpperBand", m_upperband.Value, Bars.TimeValue.ToBinary());
			m_IndicatorArea.SetValue("LowerBand", m_lowerband.Value, Bars.TimeValue.ToBinary());          
			
			ChartCustomDraw.ReDraw();
		}

		#region IChartCustomDrawer
		void IChartCustomDrawer.Draw(DrawContext context, EDrawPhases phase) {
			if (phase != EDrawPhases.Final) return;

			RectangleF _dr = context.FullRect;
			ChartPoint DrPleft = context.Environment.Point2ChartPoint(
				new PointF { X = _dr.X, Y = _dr.Y }
			);
			ChartPoint DrPRight = context.Environment.Point2ChartPoint(
				new PointF { X = _dr.Width, Y = _dr.Height }
			);

			if (DrPleft.Time.Ticks > DrPRight.Time.Ticks) {
				return;
			}

			Bar[] BarsToRedraw = null;

			context.Environment.Bars.Get(DrPleft.Time, DrPRight.Time, out BarsToRedraw);
			
			if (!object.Equals(BarsToRedraw, null)) {
				int countBars = BarsToRedraw.Length;
				if (countBars > 0) {
					AreaPainter p = new AreaPainter(fillcolor, (byte)(255 - m_intensity));
					PointF[] pf = new PointF[countBars * 2];
					int full = countBars * 2 - 1;
					for (int i = 0, idx = 0; i < countBars; i++) {
						double high0 = m_IndicatorArea.GetValue("UpperBand", BarsToRedraw[i].Time.ToBinary());
						double low0 = m_IndicatorArea.GetValue("LowerBand", BarsToRedraw[i].Time.ToBinary());

						if (high0 < 0|| low0 < 0) {
							idx = 0;
							full-=2;
							pf = new PointF[full + 1];
							continue;
						}

						Bar b = BarsToRedraw[i];
						pf[idx] = context.Environment.ChartPoint2Point(
							new ChartPoint {
								Price = high0,
								Time = b.Time
							}
						);

						pf[full - idx] = context.Environment.ChartPoint2Point(
							new ChartPoint {
								Price = low0,
								Time = b.Time
							}
						);

						idx++;
					}
					p.PaintArea(context.graphics, pf);
				}
			}
			context.DirtyRect = _dr;
		}
		#endregion
	}
}
