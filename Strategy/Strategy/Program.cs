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
			var inputResults = new List<bool>();
			inputResults.Add(true);

			inputResults.Add(true);
			inputResults.Add(true);
			inputResults.Add(false);
			inputResults.Add(true);
			inputResults.Add(true);


			//var сumulativeBetTester = new BetStrategyTester(eBetStrategies.Cumulative, inputResults);
			//сumulativeBetTester.TestStrategy();

			var resetBetTester = new BetStrategyTester(eBetStrategies.Reset, inputResults);
			resetBetTester.TestStrategy();
		}
	}
}
