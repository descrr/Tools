using System;
using System.Collections.Generic;
using System.IO;

namespace Strategy
{
	public class Bar
	{
		public Decimal O { get; set; }
		public Decimal H { get; set; }
		public Decimal L { get; set; }
		public Decimal C { get; set; }
		public DateTime Dt { get; set; }
	}

	public class BarHistory
	{
		public readonly Dictionary<int, Bar> Bars;

		public BarHistory()
		{
			Bars = new Dictionary<int, Bar>();
		}

		public void AddBar(Bar bar)
		{
			int cnt = Bars.Count;
			Bars[cnt++] = bar;
		}

	}
	public class BarsLoader
    {
		public BarHistory LoadFromFile(string fileName/*, int maxBarsCount*/)
		{
			//read all lines
			int j = 0;
			Dictionary<int, string> allLines = new Dictionary<int, string>();
            foreach (string line in File.ReadAllLines(fileName))
            {
                allLines[j++] = line;
            }

			//apply lines limit
			var barHistory = new BarHistory();
			//if(maxBarsCount > allLines.Count)
			//{
			//	maxBarsCount = allLines.Count;
			//}

			for (int i = 0 /* allLines.Count - maxBarsCount*/; i < allLines.Count; i++)
			{
				barHistory.AddBar(ConvertCsvLine2Bar(allLines[i]));
			}

			return barHistory;
		}

		private Bar ConvertCsvLine2Bar(string line)
		{
			var values =  line.Split(',');

			return new Bar
			{
				Dt = Convert.ToDateTime(values[0] + " " + values[1]),
				O = Convert.ToDecimal(values[2]),
				H = Convert.ToDecimal(values[3]),
				L = Convert.ToDecimal(values[4]),
				C = Convert.ToDecimal(values[5])
			};
		}

		//public static BarHistory LoadFromDb(string symbolName)
		//{
		//}
	}
}
