using System;
using System.Drawing;
using System.Linq;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage {
	namespace Function 	{
		public sealed class ZScore : FunctionSimple<System.Double> {
			public int length;
			
			private VolatilityStdDev volstddev;
					
			public ZScore(CStudyControl _master) : base(_master) { }
			public ZScore(CStudyControl _master, int _ds) : base(_master, _ds) { }

			protected override void Create() {
				volstddev = new VolatilityStdDev(this);
			}

			protected override void StartCalc() {
				volstddev.numdays = length;
			}

			protected override System.Double CalcBar() {
				double volatility = volstddev[0]/Math.Sqrt(252);
				double current_return = Math.Log(Bars.Close[0]/Bars.Close[1]);

				return current_return / volatility;
			}
		}
	}
}