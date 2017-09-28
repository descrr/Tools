using System;
using System.Linq;
using System.Windows.Forms;

namespace PriceGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
			Form1.RenewParameters();

			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());

			/*
            if (args != null && args.Count() > 0)
            {
                Form1.RenewPrice(args[0] == "-");
                Form1.RenewDeal();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
			*/

			//var form = new Form1();
			//if (args != null && args.Count() > 0 && args[0] != null)
			//{
			//    form.RenewPrice();
			//}
			//else
			//{
			//    form.RenewPrice();
			//    //Application.Run(form);
			//}



			//Application.Run(form);

			//form.Execute();

			//var collector = new ProductCollector();
			//collector.Execute();
			//form.Close();
		}
    }
}
