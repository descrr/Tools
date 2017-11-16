using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
	public class DirectionStrategySelector
	{
		private int TemplateNumber = (int)(Math.Pow(2, Constants.Rank));

		private Dictionary<int, bool> XoResults;
		public DirectionStrategySelector(Dictionary<int, bool> xoResults)
		{
			XoResults = xoResults;
		}

		public DirectionStrategy GetBestStrategy()
		{
			int profitCount = -1;
			DirectionStrategy bestStrategy = null; 
			for(int i = 0; i < TemplateNumber; i++)
			{
				string strategyTemplate = Convert.ToString(i, 2);

				var directionStrategy = new DirectionStrategy();
				directionStrategy.InitStrategy(strategyTemplate);

				bool forecastedDirection;
				bool prevRealDirection = XoResults[0];
				for (int j = 1; j < XoResults.Count; j++)
				{
					forecastedDirection = directionStrategy.GetNextDirection(prevRealDirection);
					prevRealDirection = XoResults[j];
				}

				if(profitCount < directionStrategy.ProfitCounter)
				{
					profitCount = directionStrategy.ProfitCounter;
					bestStrategy = directionStrategy;
				}
			}
			return bestStrategy;
		}
	}

	public class DirectionStrategy
	{
		private string StrategyTemplate;
		private int TemplateIndex;

		public bool? ForecastedDirection;
		public Dictionary<int, bool> DirectionResults = new Dictionary<int, bool>();
		public int ProfitCounter;
		
		public void InitStrategy(string strategyTemplate)
		{
			StrategyTemplate = strategyTemplate;
			ForecastedDirection = null;
			TemplateIndex = -1;
			ProfitCounter = 0;
			
		}

		public bool GetNextDirection(bool prevMarketDirection)
		{
			//renew profit counter
			if(prevMarketDirection != null 
			&& ForecastedDirection != null
			&& prevMarketDirection == ForecastedDirection)
			{
				if (prevMarketDirection == ForecastedDirection)
					++ProfitCounter;
			}

			//forecast next direction
			++TemplateIndex;
			if (TemplateIndex > StrategyTemplate.Length - 1)
				TemplateIndex = 0;

			//follow the trend
			if (StrategyTemplate[TemplateIndex] == '1')
			{
				ForecastedDirection = prevMarketDirection;
			}
			else //reverse trend direction
			{
				ForecastedDirection = !prevMarketDirection;
			}
			DirectionResults[DirectionResults.Count] = (bool)ForecastedDirection;

			return (bool)ForecastedDirection;
		}
	}
}
