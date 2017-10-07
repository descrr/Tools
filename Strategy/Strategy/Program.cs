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
			var dataLoader = new BarsLoader();
			var history = dataLoader.LoadFromFile(@"C:\Projects\Tools\Strategy\Data\EURUSD5.csv");
			var converter = new Bar2XoConverter();
			var xoList = converter.Convert(history.Bars);

			//xoList[xoList.Count] = false;

			var directionStrategySelector = new DirectionStrategySelector(xoList);
			var directionStrategy = directionStrategySelector.GetBestStrategy();
			var betSelector = new BetStrategySelector(directionStrategy.DirectionResults);
			var betStrategy = betSelector.GetBestStrategy();

			Console.WriteLine("{0} ==> Next forecast: {1}, bet: {2}", history.Bars[history.Bars.Count - 1].Dt, directionStrategy.ForecastedDirection, betStrategy.CurrentBetInUnits);
		}
	}
}
