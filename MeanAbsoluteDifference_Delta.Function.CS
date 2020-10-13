using System;

// Matthew Scheffel
// matt@dataheck.com

namespace PowerLanguage {
	namespace Function {
		public sealed class MeanAbsoluteDifference_Delta : FunctionSimple<System.Double> {
			public MeanAbsoluteDifference_Delta(CStudyControl _master) : base(_master) { }
			public MeanAbsoluteDifference_Delta(CStudyControl _master, int _ds) : base(_master, _ds) { }
			
			private AverageFC average_value;
			private FirstDifference first_difference;
			
			public int length;

			public ISeries<Double> target { get; set; }
				
			protected override void Create() {
				// create variable objects and function objects
				average_value = new AverageFC(this);
				first_difference = new FirstDifference(this);
			}

			protected override void StartCalc() { 
				first_difference.Price = target;
								
				average_value.length = length;
				average_value.price = first_difference;
			}

			protected override System.Double CalcBar() {			
				return average_value[0];
			}
		}
	}
}