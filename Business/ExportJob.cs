using Business.Helpers;
using Business.LocalModel;
using DataAccess;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Environment;

namespace Business
{
    public enum LogLevel
    {
        Information,
        Warning,
        Error
    }

    public class ExportJob : Abstract
    {

        private const string applicationName = "Farmatic Data Exporter";
        /// <summary>
        /// 'name' property of the connection string
        /// </summary>
        private string _configConnectionStringName;

        /// <summary>
        /// Configuration Storage
        /// </summary>
        public JobConfig Config { get; set; }

        /// <summary>
        /// Time (in ms) for the first execution 
        /// </summary>
        public int DueTime { get; set; }
        /// <summary>
        /// Triggering Frequency
        /// </summary>
        public double Freq { get; set; }

        /// Local Farmatic DB
        /// <summary>
        /// DB provider
        /// </summary>
        private DbProviderFactory _factory;
        /// <summary>
        /// Local DB Connection object
        /// </summary>
        private DbConnection _connection;

        //log to file
        private TextWriter log = null;

        public ExportJob() { }

        public ExportJob(string connectionStringName)
        {
            _configConnectionStringName = connectionStringName;
            //Init();            
        }

        public bool ConfigureSuccess(string connectionStringName)
        {
            _configConnectionStringName = connectionStringName;
            bool retVal = false;

            string _currDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            _currDir = System.IO.Path.GetDirectoryName(_currDir);
            Directory.SetCurrentDirectory(_currDir);
            var logDir = Path.Combine(
                Environment.GetFolderPath(SpecialFolder.CommonApplicationData),
                $"Makesoft\\{applicationName}"
                );
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = applicationName;
                eventLog.WriteEntry(
                    $"Executing directory: {_currDir}{Environment.NewLine}Log directory: {logDir}", EventLogEntryType.Information);
            }

            DateTime now = DateTime.Now;

            // obtencion de la configuracion
            var appconfig = ConfigurationManager.AppSettings;


            // hh:mm:ss de inicio
            int.TryParse(appconfig["hour"], out int configHH);
            int.TryParse(appconfig["minute"], out int configmm);
            int.TryParse(appconfig["second"], out int configss);
            double.TryParse(appconfig["periodInHours"], out double freqH);

            // periodo
            var freq = TimeSpan.FromHours(freqH).TotalMilliseconds;
            Freq = freq;

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
            DueTime = dueTime;


            var logFilePath = Path.Combine(logDir, "validate.txt");
            using (TextWriter writer = new FileInfo(logFilePath).AppendText())
            {
                var dbbCSConfig = ConfigurationManager.ConnectionStrings[connectionStringName];
                writer.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:fff")}: - {_currDir} - ExportJob.Export - start at {configHH.ToString().PadLeft('0')}:{configmm.ToString().PadLeft('0')}:{configss.ToString().PadLeft('0')} - each {TimeSpan.FromMilliseconds(Freq).TotalMinutes} minutes");
                if (!int.TryParse(ConfigurationManager.AppSettings["DaysToResend"], out int daysResend))
                    throw new Exception($"DaysToResend config was not found or invalid, it must be a positive int");
                daysResend = Math.Abs(daysResend);
                Config = new JobConfig
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
                writer.WriteLine(Config.ToString());

                retVal = ValidateConfig(out Exception ex);
                writer.WriteLine($"Valid config: {retVal}");

                if (!retVal)
                {
                    writer.WriteLine($"Exception found: {ex.ToString()}");
                }
            }

            return retVal;
        }

        public override void Init()
        {
            base.Init(_configConnectionStringName);
            _factory = DbProviderFactories.GetFactory(Config.ProviderConnectionString);
        }


        public override DbConnection BuildConnection(DbProviderFactory _factory)
        {
            return base.BuildConnection();
        }

        public override DbCommand GetCommand(DbConnection _connection)
        {
            return _connection.CreateCommand();
        }

        // completion event
        protected virtual void OnComplete(EventArgs e)
        {
            CompleteHandler?.Invoke(this, e);
        }

        public event EventHandler CompleteHandler;

        public void Export(object state)
        {
            Config = (JobConfig)state;
            StringBuilder sbEvent = new StringBuilder();

            using (log = new FileInfo($"{DateTime.Now.ToString("yyyyMMddhhmm")}-exportjoblog.txt").AppendText())
            {
                try
                {
                    if (!int.TryParse(Config.DaysToResend.ToString(), out int daysResend))
                        throw new Exception($"DaysToResend config was not found or invalid, it must be a positive int");

                    Log($"{DateTime.Now.ToString("hh:mm:ss:fff")}: ExportJob.Export -> {Config.FarmaticConnectionString} ({Config.ProviderConnectionString}) ");

                    // try webApi retrieve lastrecord to limit request
                    Stopwatch timerTokenRefresh = new Stopwatch();
                    timerTokenRefresh.Start();
                    APIMethods api = new APIMethods(Config);
                    api.SetToken();
                    if (api.TokenData == null)
                        throw new System.Exception($"Could not obtain token from WebAPI, endpoint root was {Config.APIEndpoint}");

                    // 2. Get 'last export date' (if I can add column: ExportDate, if not fallback to use FechaVenta) data from webAPI
                    var _lastExportDate = api.GetLastExportInfo();
                    DateTime searchFrom = DateTime.Now;

                    // 3. Get all records from a day before 'last export date' to now with SQL in pages of 20 records each 
                    if (_lastExportDate.Status == System.Net.HttpStatusCode.OK)
                    {
                        // searchFrom = searchFrom.AddDays(-1);
                        var result = Math.Ceiling(DateTime.Now.Subtract(_lastExportDate.DateLastVenta).TotalDays);
                        if (Config.DaysToResend < result)
                        {
                            daysResend = int.Parse(Math.Ceiling(result).ToString());
                            Log($"Getting from {daysResend} days ago: {DateTime.Now.AddDays(daysResend * -1).ToString("yyyy-MM-dd")}.");
                        }
                        else
                        {
                            Log($"Getting from {Config.DaysToResend} days ago: {DateTime.Now.AddDays(Config.DaysToResend * -1).ToString("yyyy-MM-dd")}.");
                        }
                    }
                    else if (_lastExportDate.Status == System.Net.HttpStatusCode.NotFound)
                    {
                        // no hay registros, configurar para que envie todo (5 años)
                        var dOld = DateTime.Now.AddYears(-5);
                        var result = DateTime.Now.Subtract(dOld);
                        Log($"Getting all currrent records from {dOld.ToString("yyyy-MM-dd")}");
                        daysResend = int.Parse(Math.Ceiling(result.TotalDays).ToString());
                    }

                    DALMethods dal = new DALMethods(Config);
                    dal.ConnectionOpen(out DbConnection connection);

                    var command = connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = dal._parametrizedSelectVentas;

                    SqlParameter days = new SqlParameter
                    {
                        ParameterName = "@daysToRepeat",
                        Value = daysResend * -1
                    };
                    command.Parameters.Add(days);

                    //using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess)) // for long streams
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    sbEvent.AppendLine("Send operation log:");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // conversion to DTO
                            if (VentaDTO.TryFromDBRecord(reader, out VentaDTO venta))
                            {
                                var ventaStr = venta.ToString();
                                Log(ventaStr, false);
                                sbEvent.Append(ventaStr);
                                var result = api.PostVenta(venta);
                                if (!result)
                                {
                                    var msg = $"ERROR while sending IdVenta {reader["IdentificadorVenta"]}";
                                    Log(msg, false);
                                    sbEvent.AppendLine(" - ERROR");
                                    LogToEventViewer(new Exception(msg));
                                    throw new Exception(msg);
                                }
                                else
                                {
                                    sbEvent.AppendLine(" - OK");
                                }

                                if (timerTokenRefresh.Elapsed.TotalMinutes > 4)
                                {
                                    //refresh token
                                    Log("Refreshing api token", false);
                                    api.SetToken();
                                    if (api.TokenData == null)
                                    {
                                        var ex = new System.Exception($"Could not refresh token from WebAPI, endpoint root was {Config.APIEndpoint}");
                                        LogToEventViewer(ex);
                                        throw ex;
                                    }
                                    else
                                    {
                                        timerTokenRefresh.Reset();
                                        timerTokenRefresh.Start();
                                    }
                                }
                            }
                            else
                            {
                                var msg = $"ERROR while building DTO for IdVenta record with Id: {reader["IdentificadorVenta"]}";
                                Log(msg);
                                var ex = new Exception(msg);
                                LogToEventViewer(ex);
                                throw ex;
                            }

                        }
                    }
                    timer.Stop();
                    var doneLog = $"Process completed, processing time: {timer.Elapsed.TotalSeconds} secs. {Environment.NewLine} {sbEvent.ToString()}";
                    Log(doneLog);
                }
                catch (IOException ioEx)
                {
                    // log IOex to eventlog
                    LogToEventViewer(ioEx);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Log($"{DateTime.Now.ToString("hh:mm:ss:fff")}: EX!: {ex.ToString()}");
                    }
                    catch (Exception exi)
                    {
                        // log ex to eventlog
                        LogToEventViewer(exi);
                    }
                }
            }
            OnComplete(EventArgs.Empty);
        }


        public bool ValidateConfig(out Exception outex)
        {
            bool success = false;
            outex = null;
            //check config works at least for DB
            try
            {
                Init();
                using (_connection = BuildConnection(Config.FarmaticConnectionString))
                {

                    _connection.Open();
                    Debug.WriteLine($"Connected");

                }
                Debug.WriteLine($"Disconnected");
                success = true;
            }
            catch (IOException ioEx)
            {
                // log IOex to eventlog
                LogToEventViewer(ioEx);
                outex = ioEx;
            }
            catch (Exception ex)
            {
                outex = ex;
                try
                {
                    Log($"{DateTime.Now.ToString("hh:mm:ss:fff")}: EX!: {ex.ToString()}");
                }
                catch (Exception)
                {
                    // log IOex to eventlog
                    LogToEventViewer(new IOException { Source = $"Exception: {ex.ToString()}" });
                }
            }
            return success;
        }

        private static void LogToEventViewer(Exception ioEx)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = applicationName;
                eventLog.WriteEntry($"Exception: {ioEx.ToString()}", EventLogEntryType.Error);
            }
        }

        private void Log(string msg, bool logToEventViewer = true, LogLevel level = LogLevel.Information)
        {
            Debug.WriteLine($"Business.ExportJob: {msg}");
            if (log != null) // file log
            {
                log.WriteLine($"Business.ExportJob: {msg}");
            }

            if (!logToEventViewer) return;

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = applicationName;
                switch (level)
                {
                    case LogLevel.Information:
                        eventLog.WriteEntry($"{msg}", EventLogEntryType.Information);
                        break;
                    case LogLevel.Warning:
                        eventLog.WriteEntry($"{msg}", EventLogEntryType.Warning);
                        break;
                    case LogLevel.Error:
                        eventLog.WriteEntry($"{msg}", EventLogEntryType.Error);
                        break;
                    default:
                        eventLog.WriteEntry($"{msg}", EventLogEntryType.Information);
                        break;
                }
            }
        }
    }
}
