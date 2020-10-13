using System;

// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Function {
    public class VolatilityStdDev_ClosedOnly : FunctionSimple<Double> {
        private static readonly double m_k = Math.Sqrt(252);

        public VolatilityStdDev_ClosedOnly(CStudyControl ctx) : base(ctx) {}

        public VolatilityStdDev_ClosedOnly(CStudyControl ctx, int data_stream) : base(ctx, data_stream) {}

        public Int32 numdays { get; set; }

        protected override double CalcBar() {
			var price = (ISeries<double>) null;
			
			if (!Bars.LastBarInSession) {
	            price = new Lambda<Double>(_bb => Math.Log(Bars.Close[_bb+1]/Bars.Close[1 + _bb + 1]));
			} else {
	            price = new Lambda<Double>(_bb => Math.Log(Bars.Close[_bb]/Bars.Close[1 + _bb]));
			}
			
			return price.StandardDeviationCustom(numdays, 1)*m_k;
        }
    }
}