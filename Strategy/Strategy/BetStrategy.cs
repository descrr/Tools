using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
	public enum eBetStrategies
	{
		Cumulative = 1,
		Reset = 2
	}

	public class BetStrategyTester
	{
		private BaseBetStrategy Strategy;
		private List<bool> WinResults;

		private int UnitsCount = 5; // 30..50
		private int Cycle = 5; //5..10

		public BetStrategyTester(eBetStrategies strategy, List<bool> winResults)
		{
			Strategy = CreateBetStrategy(strategy);
			Strategy.InitBetStrategy(Cycle, UnitsCount, winResults[0]);
			WinResults = winResults;
		}

		public void TestStrategy()
		{
			int winUnits = 0;
			Console.WriteLine("Turn: 1");
			
			for (int i = 1; i < WinResults.Count; i++)
			{
				if (i > 1)
					Console.WriteLine("Turn: {0}", i);

				Console.WriteLine("Before:");
				Console.WriteLine("units bet={0}", Strategy.CurrentBetInUnits);
				Console.WriteLine("units count left={0}", Strategy.UnitsCount);
				Console.WriteLine("");

				var winResult = WinResults[i];
				winUnits = Strategy.ProcessBet(winResult);
				
				Console.WriteLine("After:");
				Console.WriteLine("win units={0}", winUnits);
				Console.WriteLine("units count={0}", Strategy.UnitsCount + Strategy.CurrentBetInUnits);
				Console.WriteLine("next units bet={0}", Strategy.CurrentBetInUnits);
				Console.WriteLine("");
				Console.WriteLine("");
				if (Strategy.UnitsCount <= 0)
				{
					Console.WriteLine("Strategy has failed");
					break;
				}				
			}
		}

		private BaseBetStrategy CreateBetStrategy(eBetStrategies betStrategy)
		{
			switch(betStrategy)
			{
				case eBetStrategies.Cumulative: return new CumulativeBetStrategy();
					break;
				case eBetStrategies.Reset: return new ResetBetStrategy();
					break;
				default: return null;
			}
		}
	}

	public abstract class BaseBetStrategy
	{
		public int UnitsCount = 0;
		public int CurrentBetInUnits;

		protected int Cycle;
		protected int CycleSpin;
		protected bool LastBet;

		public void InitBetStrategy(int cycle, int unitsCount, bool firstBet)
		{
			Cycle = cycle;
			CycleSpin = 0;
			
			LastBet = firstBet;
			CurrentBetInUnits = 1;
			UnitsCount = unitsCount - CurrentBetInUnits;
		}

		public int ProcessBet(bool isWinBet)
		{
			++CycleSpin;
			if (CycleSpin > Cycle)
				CycleSpin = 1;

			//bet processing
			int spinResult = isWinBet ? ProcessWinBet() : ProcessLostBet();
			
			//prepare for next bet
			UnitsCount -= CurrentBetInUnits;

			return spinResult;
		}
		protected abstract int ProcessWinBet();
		protected abstract int ProcessLostBet();
	}

	public class CumulativeBetStrategy : BaseBetStrategy
	{
		protected override int ProcessWinBet()
		{
			int spinResult = CurrentBetInUnits*2;
			UnitsCount += spinResult;
			++CurrentBetInUnits;
			++CycleSpin;

			return spinResult;
		}

		protected override int ProcessLostBet()
		{
			int spinResult = (-1)*CurrentBetInUnits;
			--CurrentBetInUnits;
			--CycleSpin;

			return spinResult;
		}
	}

	public class ResetBetStrategy : BaseBetStrategy
	{
		protected override int ProcessWinBet()
		{
			int spinResult = CurrentBetInUnits * 2;
			UnitsCount += spinResult;
			++CurrentBetInUnits;
			++CycleSpin;

			return spinResult;
		}

		protected override int ProcessLostBet()
		{
			int spinResult = (-1) * CurrentBetInUnits;
			CurrentBetInUnits = 1;
			CycleSpin = 1;

			return spinResult;
		}
	}
}
