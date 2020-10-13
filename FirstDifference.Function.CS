using System;
using System.Drawing;
using System.Linq;

// Matthew Scheffel
// matt@dataheck.com

namespace PowerLanguage {
	namespace Function {
		public sealed class FirstDifference : FunctionSeries<System.Double> {
			public FirstDifference(CStudyControl _master) : base(_master) { }
			public FirstDifference(CStudyControl _master, int _ds) : base(_master, _ds) { }
			
			public ISeries<double> Price { private get; set; }

			protected override System.Double CalcBar() {
                return Price[0] - Price[1];
			}
		}
	}
}