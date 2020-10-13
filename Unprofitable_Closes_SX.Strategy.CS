using System;

// Original code modified from standard MultiCharts Profitable_Closes_SX
// Matthew Scheffel <matt@dataheck.com>

namespace PowerLanguage.Strategy {
    [IOGMode(IOGMode.Disabled)]
	
    public class Unprofitable_Closes_SX : SignalObject {
		private int previous_position;
		private int count;
		private double entry_price;

        private IOrderMarket m_LossClsSX;

        public Unprofitable_Closes_SX(object ctx) : base(ctx) {
        }

        [Input] public int NumProfitCloses { get; set; }

        protected override void Create() {
            m_LossClsSX = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "LossClsSX", EOrderAction.BuyToCover, OrderExit.FromAll));
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
				 
                if (PublicFunctions.DoubleGreater(Bars.Close[0], entry_price) && current_position == -1) {
                    count += 1;
                }
				
                if (count == NumProfitCloses) {
                    m_LossClsSX.Send();
                }
            }
			
			previous_position = current_position;
        }
    }
}