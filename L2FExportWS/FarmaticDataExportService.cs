using Business;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace L2FExportWS
{
    internal partial class FarmaticDataExportService : ServiceBase
    {
        private ExportJob job = new ExportJob();
        private Timer timer = null;
        private TimerCallback timerDelegate = null;
        private bool working = false;

        public FarmaticDataExportService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            job.CompleteHandler += Job_CompleteHandler;
            if (job.ConfigureSuccess("CS_FarmaticDB"))
            {
                timerDelegate = new TimerCallback(job.Export);
                timer = new Timer(timerDelegate, job.Config, job.DueTime, int.Parse(Math.Abs(job.Freq).ToString()));
            } else
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Farmatic Data Exporter";
                    eventLog.WriteEntry($"Invalid config.", EventLogEntryType.Error);
                }
                OnStop();
            }
        }

        private void Job_CompleteHandler(object sender, EventArgs e)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Farmatic Data Exporter";
                eventLog.WriteEntry($"Completed execution.", EventLogEntryType.Information);
            }
        }

        protected override void OnStop()
        {
            if (null != timer)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
        }
    }
}
