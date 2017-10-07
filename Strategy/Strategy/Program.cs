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
			
			var selector = new BetStrategySelector(xoList);
			var strategy = selector.GetBestStrategy();
		}
	}
}
