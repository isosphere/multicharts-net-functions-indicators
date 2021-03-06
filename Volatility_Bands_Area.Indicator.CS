﻿using System;
using System.Drawing;
using PowerLanguage.Function;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Indicator {
	[SameAsSymbol(true)]
	public class Volatility_Bands_Area : IndicatorObject, IChartCustomDrawer {
		[Input] public int length { get; set; }
		[Input] public double numdevs { get; set; }
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
		
		public Volatility_Bands_Area(object _ctx): base(_ctx) {
			numdevs = 2;
			length = 28;

			fillcolor = Color.FromArgb(128, 128, 255);
			transparency = 85;
		}

		private VariableSeries<Double> m_lowerband;
		private VariableSeries<Double> m_upperband;

		private IPlotObject Plot1;
		private IPlotObject Plot2;

		private AreaValue m_IndicatorArea;
		private VolatilityStdDev_ClosedOnly volstddev;

		private bool isCSDRegistred;
    	private byte m_intensity; // 0-255

		protected override void Create() {
			m_lowerband = new VariableSeries<Double>(this);
			m_upperband = new VariableSeries<Double>(this);
			volstddev = new VolatilityStdDev_ClosedOnly(this);
			
			Plot1 = AddPlot(new PlotAttributes("UpperBand", 0, Color.Green, Color.Empty, 1, 0, true));
			Plot2 = AddPlot(new PlotAttributes("LowerBand", 0, Color.Red, Color.Empty, 1, 0, true));

			m_IndicatorArea = new AreaValue();
			isCSDRegistred = false;
		}	

		protected override void StartCalc() {
			if (!isCSDRegistred) {
				ChartCustomDraw.Register(this);
				isCSDRegistred = true;
			}
			
			volstddev.numdays = length;
		}

		protected override void StopCalc() {
			if (isCSDRegistred) {
				ChartCustomDraw.Unregister(this);
				isCSDRegistred = false;
			}
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
