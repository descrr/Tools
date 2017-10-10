using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
	public class Bar2XoConverter
	{
		private const int BoxSize = 50;
		private const int Point = 10000;

		private Dictionary<int, bool> xoList = new Dictionary<int, bool>();
		private Decimal CurrentLevel;
		
		public Dictionary<int, bool> Convert(Dictionary<int, Bar> bars)
		{
			//Find current level
			int currentLevel = (int)(bars[0].O*(Decimal)Point);
			int rest = currentLevel % BoxSize;
			int mult = (currentLevel - rest) / BoxSize;
			int nextDwnLevel = mult * BoxSize;
			int nextUpLevel = nextDwnLevel + BoxSize;

			int barIndex = 0;

			//Convert bars
			for (int i = 0; i < bars.Count; i++)
			{
				var bar = bars[i];

				//up level has been reached
				if(bar.H > (Decimal)((Decimal)nextUpLevel) / (Decimal)Point)
				{
					xoList[barIndex++] = true;
					nextUpLevel += BoxSize;
					nextDwnLevel = nextUpLevel - 2 * BoxSize;
				}
				//down level has been reached
				else if (bar.L < (Decimal)((Decimal)nextDwnLevel)/(Decimal)Point)
				{
					xoList[barIndex++] = false;
					nextDwnLevel -= BoxSize;
					nextUpLevel = nextDwnLevel + 2 * BoxSize;
					
				}
			}
			

			return xoList;
		}
	}
}
