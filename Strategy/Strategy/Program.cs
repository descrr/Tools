using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
	class Program
	{
		static void Main(string[] args)
		{
			if(args.Count() == 0)
			{
				Console.WriteLine("No input parameters");
				return;
			}

			string currencyPair = args[0];
			Console.WriteLine(currencyPair);

			
			int maxXoCount = Constants.MaxXoCount;			
			var dataLoader = new BarsLoader();
			string fileName = string.Format(@"..\..\..\Data\{0}5.csv", currencyPair);
			var history = dataLoader.LoadFromFile(fileName);
			var converter = new Bar2XoConverter();
			var xoListMain = converter.Convert(history.Bars, Constants.MaxXoCount);
			//const string strategyTestTemplate = "111001000100111";

			var modes = new List<string>();
			modes.Add("Current"); //for current position
			modes.Add("1");
			modes.Add("11");
			modes.Add("111");
			modes.Add("0");
			modes.Add("00");
			modes.Add("000");			

			foreach (var mode in modes)
			{
				DirectionStrategy bestDirectionStrategy = null;
				for (int i = Constants.MinRank; i <= Constants.MaxRank; i++)
				{
					var strategy = ProcessRank(mode, i - 1, currencyPair, xoListMain);

					//if (mode == "0" && bestDirectionStrategy != null &&
					//	(strategyTestTemplate == strategy.StrategyTemplate
					//	|| strategyTestTemplate == bestDirectionStrategy.StrategyTemplate)
					//)
					//{
					//	int test = 9;
					//	++test;
					//}

					if (bestDirectionStrategy == null || strategy.ProfitCounter > bestDirectionStrategy.ProfitCounter)
					{
						bestDirectionStrategy = strategy;
					}
				}
				Console.WriteLine("Template={0}", bestDirectionStrategy.StrategyTemplate);
				Console.WriteLine("Mode {0}, Forecasted: {1}, ProfitCounter={2}", mode, bestDirectionStrategy.ForecastedDirection, bestDirectionStrategy.ProfitCounter);
				Console.WriteLine("");
			}
		}

		private static DirectionStrategy ProcessRank(string mode, int startXoIndex, string currencyPair, Dictionary<int, bool> xoListIn)
		{
			var dtStart = DateTime.Now;
			var xoListMain = new Dictionary<int, bool>(xoListIn);

			//xoListMain.Remove(xoListMain.Count - 1);
			//xoListMain.Remove(xoListMain.Count - 1);

			switch (mode)
			{
				case "1":
					xoListMain[xoListMain.Count] = true;
					break;
				case "11":
					xoListMain[xoListMain.Count] = true;
					xoListMain[xoListMain.Count] = true;
					break;
				case "111":
					xoListMain[xoListMain.Count] = true;
					xoListMain[xoListMain.Count] = true;
					xoListMain[xoListMain.Count] = true;
					break;
				case "0":
					xoListMain[xoListMain.Count] = false;
					break;
				case "00":
					xoListMain[xoListMain.Count] = false;
					xoListMain[xoListMain.Count] = false;
					break;
				case "000":
					xoListMain[xoListMain.Count] = false;
					xoListMain[xoListMain.Count] = false;
					xoListMain[xoListMain.Count] = false;
					break;
			}

			var applyConverter = new Bar2XoConverter();
			applyConverter.ApplyLimits(xoListMain, Constants.MaxXoCount);
			xoListMain = applyConverter.XoList;

			int winCount = 0;
			int winInUnits = 0;
			DirectionStrategy selectedDirectionStrategy = null;
			for (int i = startXoIndex; i < xoListMain.Count; i++)
			{
				//copy first i elements
				var xoList = new Dictionary<int, bool>();
				for (int j = 0; j <= i; j++)
				{
					if (j < xoListMain.Count)
					{
						xoList.Add(j, xoListMain[j]);
					}
				}

				//if (i == xoListMain.Count - 1)
				//{
				//	int rett = 9;
				//}

				var directionStrategySelector = new DirectionStrategySelector(xoList, startXoIndex);
				var directionStrategy = directionStrategySelector.GetBestStrategy();

				/*
				var betSelector = new BetStrategySelector(directionStrategy.DirectionResults);
				var betStrategy = betSelector.GetBestBetStrategy();

				bool? realValue = null;
				if (i < xoListMain.Count - 1)
				{
					realValue = xoListMain[i];
					//correct forecasting
					if (directionStrategy.ForecastedDirection == realValue)
					{
						winCount++;
						winInUnits += betStrategy.CurrentBetInUnits;
					}
					else // wrong forecasting
					{
						winCount--;
						winInUnits -= betStrategy.CurrentBetInUnits;
					}
				}*/
				//else
				//{
				//	int ret = 0;
				//}

				//Console.WriteLine("Forecasted: {0}, real value: = {1}, winCount={2}, winInUnits={3}, BetStrategyT = ype={4}, BetCycle={5}", directionStrategy.ForecastedDirection, realValue, winCount, winInUnits, betStrategy.StrategyType, betStrategy.Cycle);
				//Console.WriteLine("{0} of {1}", i, xoListMain.Count);

				selectedDirectionStrategy = directionStrategy;
			}

			var dtEnd = DateTime.Now;
			TimeSpan durationInterval = dtEnd - dtStart;
			string duration = string.Format("{0}:{1}", durationInterval.Minutes, durationInterval.Seconds);
			
			Console.WriteLine("Rank {0}, Forecasted: {1}, ProfitCounter={2}, duration: {3}", startXoIndex+1, selectedDirectionStrategy.ForecastedDirection, selectedDirectionStrategy.ProfitCounter, duration);
			return selectedDirectionStrategy;
		}
	}
}
