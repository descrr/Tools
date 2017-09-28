using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceGenerator
{
    public abstract class LinksGenerator
    {
        public LinksGenerator()
        {
            Init();
        }

        protected List<string> Storage = new List<string>();

        private int Index = 0;

        protected abstract void Init();

        public string GetLink()
        {
            if(Index >= Storage.Count)
            {
                Index= 0;
            }
            
            return Storage[Index++];
        }


    }
}
