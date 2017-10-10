﻿using System;
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
			var dataLoader = new BarsLoader();
			var history = dataLoader.LoadFromFile(@"C:\Projects\Tools\Strategy\Data\EURUSD5.csv");
			var converter = new Bar2XoConverter();
			var xoListMain = converter.Convert(history.Bars);

			xoListMain[xoListMain.Count] = true;//////////

			//skip first 12 and test
			int startXoIndex = 12;// xoListMain.Count - 1;//12;
			int winCount = 0;
			int winInUnits = 0;

			for(int i = startXoIndex; i < xoListMain.Count; i++)
			{
				//copy first i elements
				var xoList = new Dictionary<int, bool>();
				for(int j = 0; j <= i; j++)
				{
					xoList.Add(j, xoListMain[j]);
				}

				var directionStrategySelector = new DirectionStrategySelector(xoList);
				var directionStrategy = directionStrategySelector.GetBestStrategy();
				var betSelector = new BetStrategySelector(directionStrategy.DirectionResults);
				var betStrategy = betSelector.GetBestStrategy();

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
				}

				Console.WriteLine("Forecasted: {0}, real value: = {1}, winCount={2}, winInUnits={3}", directionStrategy.ForecastedDirection, realValue, winCount, winInUnits);
			}
			//Console.WriteLine("{0} ==> Next forecast: {1}, bet: {2}", history.Bars[history.Bars.Count - 1].Dt, directionStrategy.ForecastedDirection, betStrategy.CurrentBetInUnits);
		}
	}
}
