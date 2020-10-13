using System;

// Matthew Scheffel, matt@dataheck.com
// Code used "vwapResettable" as a base

namespace PowerLanguage.Function {
    public class vwapSession : FunctionSimple<Double> {
        private VariableObject<Double> m_pricew;
        private VariableObject<Double> m_sharew;
		private VariableObject<Boolean> prior_bar_final;

        public vwapSession(CStudyControl ctx):base(ctx) {}
        public vwapSession(CStudyControl ctx, int data_stream):base(ctx, data_stream) {}

        public ISeries<Double> price { get; set; }

        protected override void Create() {
            m_pricew = new VariableObject<Double>(this);
            m_sharew = new VariableObject<Double>(this);
			prior_bar_final = new VariableObject<Boolean>(this);
			
			prior_bar_final.Value = false; // initialize
        } 

        protected override double CalcBar() {
			if (prior_bar_final.Value) {
				m_pricew.Value = 0;
                m_sharew.Value = 0;
				prior_bar_final.Value = Bars.LastBarInSession;
				
				return price.Value;
			} else {
	            m_pricew.Value += price.Value*Bars.Ticks[0];
	            m_sharew.Value += Bars.Ticks[0];				
				prior_bar_final.Value = Bars.LastBarInSession;
				return m_pricew.Value/m_sharew.Value;
			}
        }
    }
}