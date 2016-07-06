using System;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using System.IO;

namespace RedditService
{
    public partial class RedditService : ServiceBase
    {
        public Timer Schedular { get; private set; }

        public RedditService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.WriteToFile($"Reddit Bot has started!");
            this.ScheduleService();
        }
        
        protected override void OnStop()
        {
            this.WriteToFile($"Reddit Bot has stopped!");
            this.Schedular.Dispose();
        }

        private void WriteToFile(string text)
        {
            string path = @"C:\RedditBotLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }

        private void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = ConfigurationManager.AppSettings["Mode"].ToUpper();

                DateTime scheduledTime = DateTime.MinValue;
                if (mode == "DAILY")
                {
                    scheduledTime = DateTime.Parse(ConfigurationManager.AppSettings["ScheduledTime"]);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }

                    TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                    string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                    this.WriteToFile("Reddit Bot scheduled to run after: " + schedule + " {0}");

                    //Get the difference in Minutes between the Scheduled and Current Time.
                    int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                    //Change the Timer's Due Time.
                    Schedular.Change(dueTime, Timeout.Infinite);
                }
            }
            catch(Exception ex)
            {
                WriteToFile("Reddit Bot Error on: {0} " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (ServiceController serviceController = new ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SchedularCallback(object state)
        {
            this.WriteToFile("Reddit Bot log: {0}");
            this.ScheduleService();
        }
    }
}
