using System;
using System.Drawing;
using System.Linq;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage {
	namespace Function 	{
		public sealed class ZScoreVolume : FunctionSimple<System.Double> {
			public int length;
			public bool skip_current;
			
			private AverageFC average;
					
			public ZScoreVolume(CStudyControl _master) : base(_master) { }
			public ZScoreVolume(CStudyControl _master, int _ds) : base(_master, _ds) { }

			protected override void Create() {
				average = new AverageFC(this);
			}
			protected override void StartCalc() {
				average.length = length;
				average.price = Bars.Volume;
			}

			protected override System.Double CalcBar() {
				double volatility = Bars.Volume.StandardDeviationCustom(length, 1);
				if (volatility <= 0.01) {
					return 0.0;
				} else {
					return (Bars.Volume[skip_current ? 1 : 0] - average[0]) / volatility;
				}
			}
		}
	}
}