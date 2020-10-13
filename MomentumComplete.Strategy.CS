using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;

namespace PowerLanguage.Strategy {
	public class MomentumComplete : SignalObject {
		public MomentumComplete(object _ctx):base(_ctx ){}
		
		private IOrderMarket long_order, short_order, close_long_order, close_short_order;
		private VariableSeries<Double> momentum_fast, momentum_slow;
		private ISeries<double> Price { get; set; }
		
		// Fast momentum is used to close positions. Slow momentum opens them.
		[Input] public int momentum_fast_length { get; set; }
		[Input] public int momentum_slow_length { get; set; }
		
		// The offset is applied above and below the momentum, it acts as a buffer before action is taken.
		[Input] public double momentum_fast_offset { get; set; }
		[Input] public double momentum_slow_offset { get; set; }
		
		[Input] public bool enable_long { get; set; }
		[Input] public bool enable_short { get; set; }
		
		protected override void Create() {
			momentum_fast = new VariableSeries<Double>(this);
			momentum_slow = new VariableSeries<Double>(this);
			
			long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Buy));
			short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.SellShort));
			close_long_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.Sell));
			close_short_order = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, EOrderAction.BuyToCover));

			enable_long = true;
			enable_short = true;
			
			momentum_fast_length = 8;
			momentum_slow_length = 60;
			
			momentum_fast_offset = 1;
			momentum_slow_offset = 2;			
		}
		protected override void StartCalc() {
			Price = Bars.Close;
		}
		protected override void CalcBar(){
			momentum_fast.Value = Price.Momentum(momentum_fast_length);
			momentum_slow.Value = Price.Momentum(momentum_slow_length);
			
			double momentum_fast_acceleration = momentum_fast.Momentum(1);
			double momentum_slow_acceleration = momentum_slow.Momentum(1);
			
			bool close_long, close_short, open_long, open_short;
			
			close_long = (momentum_fast.Value < 0 - momentum_fast_offset) && (momentum_fast_acceleration < 0);
			close_short = (momentum_fast.Value > 0 + momentum_fast_offset) && (momentum_fast_acceleration > 0);
			open_long = enable_long && (momentum_slow.Value > 0 + momentum_slow_offset) && (momentum_slow_acceleration > 0);
			open_short = enable_short && (momentum_slow.Value < 0 - momentum_slow_offset) && (momentum_slow_acceleration < 0);		
			
			if (StrategyInfo.MarketPosition == 0) {
				if (open_long) long_order.Send();
				if (open_short) short_order.Send();
			}
			else if (StrategyInfo.MarketPosition < 0) {
				if (close_short) close_short_order.Send();
			}
			else if (StrategyInfo.MarketPosition > 0) {
				if (close_long) close_long_order.Send();
			}
		}
	}
}