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

			//skip first X and test
			int startXoIndex = Constants.Rank - 1;
			int maxXoCount = Constants.MaxXoCount;

			var dataLoader = new BarsLoader();
			string fileName = string.Format(@"..\..\..\Data\{0}5.csv", currencyPair);
			var history = dataLoader.LoadFromFile(fileName);
			var converter = new Bar2XoConverter();
			var xoListMain = converter.Convert(history.Bars, maxXoCount);

			//xoListMain[xoListMain.Count] = true;//////////

			int winCount = 0;
			int winInUnits = 0;
			for (int i = startXoIndex; i <= xoListMain.Count; i++)
			{
				//copy first i elements
				var xoList = new Dictionary<int, bool>();
				for(int j = 0; j <= i; j++)
				{
					if (j < xoListMain.Count)
					{
						xoList.Add(j, xoListMain[j]);
					}
				}

				var directionStrategySelector = new DirectionStrategySelector(xoList);
				var directionStrategy = directionStrategySelector.GetBestStrategy();
				var betSelector = new BetStrategySelector(directionStrategy.DirectionResults);
				var betStrategy = betSelector.GetBestBetStrategy();

				bool? realValue = null;
				if (i < xoListMain.Count)
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
				}
				Console.WriteLine("Forecasted: {0}, real value: = {1}, winCount={2}, winInUnits={3}", directionStrategy.ForecastedDirection, realValue, winCount, winInUnits);
			}
		}
	}
}
