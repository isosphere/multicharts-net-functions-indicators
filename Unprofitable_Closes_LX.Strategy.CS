using System;

// Original code modified from standard MultiCharts Profitable_Closes_LX
// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Strategy {
    [IOGMode(IOGMode.Disabled)]
	
    public class Unprofitable_Closes_LX : SignalObject {
		private int previous_position;
		private int count;
		private double entry_price;
        
        private IOrderMarket m_LossClsLX;

        public Unprofitable_Closes_LX(object ctx) : base(ctx) {            
        }

        [Input] public int NumProfitCloses { get; set; }

        protected override void Create() {
            m_LossClsLX = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "LossClsLX", EOrderAction.Sell, OrderExit.FromAll));
        }
		
		protected override void StartCalc() {
			NumProfitCloses = 5;
			previous_position = 0;
			count = 0;
		}

        protected override void CalcBar() {
            int current_position = Math.Sign(StrategyInfo.MarketPosition);
			
			// we arn't flat
            if (current_position != 0) {
				// we changed between long/short, reset count
                if (current_position != previous_position) {
                    count = 0;
                    entry_price = this.EntryPrice();
                }
				
                if (PublicFunctions.DoubleLess(Bars.Close[0], entry_price) && current_position == 1) {
                    count += 1;
                }
				
                if (count == NumProfitCloses) {
                    m_LossClsLX.Send();
                }
            }
			
			previous_position = current_position;
        }
    }
}