using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

public enum OptionType {
	Call,
	Put
}

public enum StrikeType {
	AtTheMoney,
	Fixed
}

namespace PowerLanguage.Indicator {		
	public class BlackScholes : IndicatorObject {
		[Input] public int days_to_expiration { get; set; }
		[Input] public int volatility_lookback { get; set; }
		[Input] public double risk_free_rate { get; set; }
		[Input] public OptionType option_type { get; set; }
		[Input] public StrikeType strike_type { get; set; }
		[Input] public double fixed_strike { get; set; }
		
		public BlackScholes(object _ctx):base(_ctx){
			days_to_expiration = 28;
			volatility_lookback = 28;
			risk_free_rate = 0.30/100.0;
			option_type = OptionType.Call;
			strike_type = StrikeType.AtTheMoney;
			fixed_strike = 100.0;			
		}
		private IPlotObject plot1;
		private StandardDeviation volatility;
		private static readonly double m_k = Math.Sqrt(252);
		
		/// https://gist.github.com/achvaicer/598242286181f5c501498a645e96f8ac
		/// @achvaicer	
		/// Adapted for MultiCharts by Matthew Scheffel matt@dataheck.com
		/* The Black and Scholes (1973) Stock option formula

		 	S = Stock price
			X = Strike price
			T = Years to maturity
			r = Risk-free rate
			v = Volatility
		*/
		public double BlackScholes_c(OptionType CallPutFlag, double S, double X, double T, double r, double v) {
			double d1 = 0.0;
			double d2 = 0.0;
			double dBlackScholes = 0.0;
			
			d1 = (Math.Log(S / X) + (r + v * v / 2.0) * T) / (v * Math.Sqrt(T));
			d2 = d1 - v * Math.Sqrt(T);
			if (CallPutFlag == OptionType.Call)	{				
				dBlackScholes = S * CND(d1) - X * Math.Exp(-r * T) * CND(d2);
			} else if (CallPutFlag == OptionType.Put) {
				dBlackScholes = X * Math.Exp(-r * T) * CND(-d2) - S * CND(-d1);				
			}
			return dBlackScholes;
		}
		
		public double CND(double X)	{
			double L = 0.0;
			double K = 0.0;
			double dCND = 0.0;
			const double a1 = 0.31938153; 
			const double a2 = -0.356563782; 
			const double a3 = 1.781477937;
			const double a4 = -1.821255978;
			const double a5 = 1.330274429;
			
			L = Math.Abs(X);
			K = 1.0 / (1.0 + 0.2316419 * L);
			dCND = 1.0 - 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-L * L / 2.0) * (a1 * K + a2 * K  * K + a3 * Math.Pow(K, 3.0) + a4 * Math.Pow(K, 4.0) + a5 * Math.Pow(K, 5.0));
			
			if (X < 0) {
				return 1.0 - dCND;
			} else {
				return dCND;
			}
		}			
		
		protected override void Create() {
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("", EPlotShapes.Line, Color.Red));
			volatility = new StandardDeviation(this);
		}
		protected override void StartCalc() {
			volatility.length = volatility_lookback;
			volatility.values = Bars.Close;

		}
		protected override void CalcBar(){
			// indicator logic 
			double vol = m_k*volatility[0];
			double strike;
			
			if (strike_type == StrikeType.AtTheMoney) {
				strike = Bars.Close[0];
			} else {
				strike = fixed_strike;
			}
			
			double black_scholes = BlackScholes_c(option_type, Bars.Close[0], strike, ((double) days_to_expiration)/365.0, risk_free_rate, vol);
			plot1.Set(black_scholes);
		}
	}
}