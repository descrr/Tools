using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
	public enum eBetStrategyTypes
	{
		Cumulative = 1,
		Reset = 2
	}

	public class BetStrategySelector
	{
		const int MinCycle = 5;
		const int MaxCycle = 10;

		private Dictionary<int, bool> WinResults;
		public BetStrategySelector(Dictionary<int, bool> winResults)
		{
			WinResults = winResults;
		}

		public BaseBetStrategy GetBestBetStrategy()
		{
			var strategyTypes = new List<eBetStrategyTypes>();
			strategyTypes.Add(eBetStrategyTypes.Cumulative);
			strategyTypes.Add(eBetStrategyTypes.Reset);
			int bestUnits = -1;
			BetStrategyTester resultStrategyTester = null;

			for (int cycle = MinCycle; cycle <= MaxCycle; cycle++)
			{
				foreach(var strategyType in strategyTypes)
				{
					var betTester = new BetStrategyTester(strategyType, cycle, WinResults);
					int units = betTester.TestStrategy();
					if(bestUnits < units)
					{
						bestUnits = units;
						resultStrategyTester = betTester;
					}
				}
			}

			return resultStrategyTester.Strategy;
		}
	}

	public class BetStrategyTester
	{
		public BaseBetStrategy Strategy;
		private Dictionary<int, bool> WinResults;

		private int UnitsCount = 30; // 30..50
		
		public BetStrategyTester(eBetStrategyTypes strategyType, int cycle, Dictionary<int, bool> winResults)
		{
			WinResults = winResults;
			Strategy = CreateBetStrategy(strategyType);
			Strategy.InitBetStrategy(cycle, UnitsCount, winResults[0]);
		}

		public int TestStrategy()
		{
			int winUnits = 0;
			//Console.WriteLine("Turn: 1");
			
			for (int i = 1; i < WinResults.Count; i++)
			{
				//if (i > 1)
				//	Console.WriteLine("Turn: {0}", i);

				//Console.WriteLine("Before:");
				//Console.WriteLine("units bet={0}", Strategy.CurrentBetInUnits);
				//Console.WriteLine("units count left={0}", Strategy.UnitsCount);
				//Console.WriteLine("");

				var winResult = WinResults[i];
				winUnits = Strategy.ProcessBet(winResult);
				
				//Console.WriteLine("After:");
				//Console.WriteLine("win units={0}", winUnits);
				//Console.WriteLine("units count={0}", Strategy.UnitsCount + Strategy.CurrentBetInUnits);
				//Console.WriteLine("next units bet={0}", Strategy.CurrentBetInUnits);
				//Console.WriteLine("");
				//Console.WriteLine("");
				if (Strategy.UnitsCount <= 0)
				{
					Console.WriteLine("Strategy has failed");
					break;
				}				
			}
			
			//return the rest of units
			Strategy.UnitsCount += Strategy.CurrentBetInUnits;
			//Console.WriteLine("units={0}", Strategy.UnitsCount);
			//Console.WriteLine("");

			return Strategy.UnitsCount;
		}

		private BaseBetStrategy CreateBetStrategy(eBetStrategyTypes betStrategy)
		{
			switch(betStrategy)
			{
				case eBetStrategyTypes.Cumulative: return new CumulativeBetStrategy();
				case eBetStrategyTypes.Reset: return new ResetBetStrategy();
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
