using System;
using System.Drawing;
using System.Linq;

// Matthew Scheffel
// matt@dataheck.com

namespace PowerLanguage {
	namespace Function {	
		public sealed class BarNumberForDateTime : FunctionSimple<System.Int32> {
			// input
			public DateTime target_date;

			// internal
			private DateTime? prior_date; // used to avoid recalculation
			private System.Int32 prior_value;			
			
			public BarNumberForDateTime(CStudyControl _master) : base(_master) { }
			public BarNumberForDateTime(CStudyControl _master, int _ds) : base(_master, _ds) { }

			protected override void Create() {
			}

			protected override void StartCalc()	{
				prior_date = null;
				prior_value = 0;
			}

			protected override System.Int32 CalcBar() {
				if (target_date != prior_date) {										
					// first, cheap check - is this data deep enough to contain the target?
					int maximum = ExecInfo.MaxBarsBack > 0 ? Math.Min(ExecInfo.MaxBarsBack, Bars.FullSymbolData.Count) : Bars.FullSymbolData.Count; 
					
					if (Bars.FullSymbolData.Time[maximum - 1] > target_date) {
						throw new System.NotImplementedException(String.Format("Not enough history to find given date. Oldest date is {0}", Bars.FullSymbolData.Time[maximum - 1]));
					}
					
					// okay, if we are here, it's possible to find it. begin binary search.
					int L = 0;
					int R = maximum - 1;
					int m = 0;
					
					while (L <= R) {
						m = (int) Math.Floor(((double) L + (double) R) / 2.00);
						
						if (Bars.FullSymbolData.Time[m] > target_date) {
							L = m + 1;
						} else if (Bars.FullSymbolData.Time[m] < target_date) {
							R = m - 1;
						} else {
							break;
						}
					}
					prior_value = m;
					prior_date = target_date;
					return m;					
				} else {
					return prior_value;
				}
			}
		}
	}
}