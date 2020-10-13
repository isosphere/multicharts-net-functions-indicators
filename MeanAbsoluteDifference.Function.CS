using System;

// Matthew Scheffel
// matt@dataheck.com

namespace PowerLanguage {
	namespace Function {
		public sealed class MeanAbsoluteDifference : FunctionSimple<System.Double> {
			public MeanAbsoluteDifference(CStudyControl _master) : base(_master) { }
			public MeanAbsoluteDifference(CStudyControl _master, int _ds) : base(_master, _ds) { }
			
			private AverageFC average_value;
			public int length;

			public ISeries<Double> target { get; set; }

			protected override void Create() {
				// create variable objects and function objects
				average_value = new AverageFC(this);
			}

			protected override void StartCalc() {
				average_value.length = length;
				average_value.price = target;
			}

			protected override System.Double CalcBar() {
				double cumulative_sum = 0.0;
					
				for (int i = 1; i < length; i++) {
					cumulative_sum += Math.Abs(target[i] - average_value[0]);
				}
				
				return cumulative_sum/length;
			}
		}
	}
}