using Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using static System.Environment;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestBusiness
    {
        private bool jobCompleted = false;

        private ExportJob job = null;
        private Timer timer = null;
        private TimerCallback timerDelegate = null;

        [TestMethod]
        public void OnStartTest()
        {
            string[] args = new string[] { };
            DateTime now = DateTime.Now;
            //obtencion de la configuracion

            // obtencion de la configuracion
            var appconfig = ConfigurationManager.AppSettings;

            // hh:mm:ss de inicio
            int.TryParse(appconfig["hour"], out int configHH);
            int.TryParse(appconfig["minute"], out int configmm);
            int.TryParse(appconfig["second"], out int configss);
            double.TryParse(appconfig["periodInHours"], out double freqH);

            // periodo
            var freq = TimeSpan.FromHours(freqH).TotalMilliseconds;

            var dueTime = Convert.ToInt32(TimeSpan.FromMilliseconds(
                new DateTime(now.Year, now.Month, now.Day, configHH, configmm, configss)
                .Subtract(now).TotalMilliseconds
                ).TotalMilliseconds);
            if (dueTime < 0)
            {
                dueTime = Convert.ToInt32(TimeSpan.FromMilliseconds(
                new DateTime(now.Year, now.Month, now.Day, configHH, configmm, configss)
                .AddDays(1).Subtract(now).TotalMilliseconds
                ).TotalMilliseconds);
            }


            var logDir = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData),
                "Makesoft", "Farmatic Export Data");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            job = new ExportJob("CS_FarmaticDB");
            using (TextWriter writer = new FileInfo(Path.Combine(logDir, "log.txt")).AppendText())
            {
                var dbbCSConfig = ConfigurationManager.ConnectionStrings["CS_FarmaticDB"];
                writer.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:fff")}: ExportJob.Export - start at {configHH}:{configmm}:{configss}");
                if (!int.TryParse(ConfigurationManager.AppSettings["DaysToResend"], out int daysResend))
                    throw new Exception($"DaysToResend config was not found or invalid, it must be a positive int");
                daysResend = Math.Abs(daysResend);
                job.Config = new JobConfig
                {
                    FarmaticConnectionString = dbbCSConfig.ConnectionString,
                    ProviderConnectionString = dbbCSConfig.ProviderName,
                    APIEndpoint = ConfigurationManager.AppSettings["APIEndpoint"],
                    APIUser = ConfigurationManager.AppSettings["APIUser"],
                    APIPwd = ConfigurationManager.AppSettings["APIPwd"],
                    JWTAuthRoute = ConfigurationManager.AppSettings["APITokenEndpoint"],
                    APIGetVentaData = ConfigurationManager.AppSettings["APIGetVentaData"],
                    APIPostVentaData = ConfigurationManager.AppSettings["APIPostVentaData"],
                    APIPostVentaDataRange = ConfigurationManager.AppSettings["APIPostVentaDataRange"],
                    APICodUsuario = ConfigurationManager.AppSettings["APICodUsuario"],
                    DaysToResend = daysResend
                };

                writer.WriteLine(job.Config.ToString());

                bool retVal = job.ValidateConfig(out Exception ex);
                writer.WriteLine($"Valid config: {retVal}");

                if (!retVal)
                {
                    writer.WriteLine($"Exception found: {ex.ToString()}");
                }
            }


            job.CompleteHandler += Job_CompleteHandler;

            timerDelegate = new TimerCallback(job.Export);
            timer = new Timer(timerDelegate, new
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["CS_FarmaticDB"],
                APIEndpoint = ConfigurationManager.AppSettings["APIEndpoint"]
            }, dueTime, int.Parse(Math.Abs(freq).ToString()));

            while (!jobCompleted)
            {
                Thread.CurrentThread.Join(300);
            }
        }

        private void Job_CompleteHandler(object sender, EventArgs e)
        {
            jobCompleted = true;
        }
    }
}
