using System;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.IO;

namespace PriceGenerator
{
    public partial class Form1 : Form
    {
        static Timer _timer;
        static Timer _timerDeal;
        public Form1()
        {
            InitializeComponent();
        }

        public static void RenewPrice(bool reloadAllProducts = false)
        {
            try
            {
                var collector = new ProductCollector();
                collector.RenewPrice(reloadAllProducts);
            }
            catch (Exception e)
            {
                string logFileName = string.Format(@"{0}\log.txt", Directory.GetCurrentDirectory());
                //if (File.Exists(logFileName))
                //    File.Delete(logFileName);

                Logger.LogMessage(e.Message);
                Logger.LogMessage(e.StackTrace);
                
                //File.WriteAllText(logFileName, e.Message);
            }
        }

        public static void RenewDeal()
        {
            var dealGenerator = new DealImportGenerator();
            dealGenerator.GeneratePrice();
        }

        private void StartTimer()
        {
            _timer = new Timer();
            _timer.Tick += TimerEventProcessor;
            _timer.Interval = 30 * 60 * 1000;
            _timer.Start();
        }

        private void StartTimerDeal()
        {
            _timerDeal = new Timer();
            _timerDeal.Tick += TimerEventProcessorDeal;
            _timerDeal.Interval = 59 * 60 * 1000;
            _timerDeal.Start();
        }
        
        private void buttonStartTimer_Click(object sender, EventArgs e)
        {
            buttonStartTimer.Enabled = false;
            RenewPrice();
            //RenewDeal();
            buttonStartTimer.Enabled = true;
            StartTimer();

            buttonDeal_Click(null, null);
        }

        private static void TimerEventProcessorDeal(Object myObject, EventArgs myEventArgs)
        {
            _timerDeal.Stop();

            try
            {
                RenewDeal();
            }
            catch (Exception)
            {

            }

            _timerDeal.Enabled = true;
        }

        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            _timer.Stop();

            try
            {
                RenewPrice();
            }
            catch(Exception)
            {

            }
            
            _timer.Enabled = true;            
        }

        private void buttonFullReload_Click(object sender, EventArgs e)
        {
            RenewPrice(true);
        }

        private void buttonDeal_Click(object sender, EventArgs e)
        {
            buttonDeal.Enabled = false;
            RenewDeal();
            buttonDeal.Enabled = true;

            StartTimerDeal();
        }

        private void buttonStartWithCleaning_Click(object sender, EventArgs e)
        {
            buttonStartWithCleaning.Enabled = false;
            RenewPrice(true);
            RenewDeal();
            buttonStartWithCleaning.Enabled = true;
            StartTimer();
        }

		private void button1_Click(object sender, EventArgs e)
		{
			buttonRenewDeal.Enabled = false;
			RenewDeal();
			buttonRenewDeal.Enabled = true;
		}

		private void buttonRenewParameters_Click(object sender, EventArgs e)
		{
			buttonRenewParameters.Enabled = false;
			RenewParameters();
			buttonRenewParameters.Enabled = true;
		}

		public static void RenewParameters()
		{
			var parametersManager = new ParametersManager();
			parametersManager.RenewParameters();
		}
	}
}
