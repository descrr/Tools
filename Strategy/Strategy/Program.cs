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
			inputResults.Add(true);
			inputResults.Add(false);

			var betTester = new BetStrategyTester(eBetStrategies.Cummulative, inputResults);
			betTester.TestStrategy();
		}
	}
}
