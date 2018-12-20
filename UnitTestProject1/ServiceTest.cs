﻿using Business;
using Business.LocalModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnitTestProject1.LocalBusiness;

namespace UnitTestProject1
{
    [TestClass]
    public class ServiceTest
    {
        private JobConfig _config;
        private ExportJob job = new ExportJob();
        private Timer timer = null;
        private TimerCallback timerDelegate = null;

        [TestMethod]
        public void TestOnStart()
        {
            if (job.ConfigureSuccess("CS_FarmaticDB"))
            {
                var freq = int.Parse(Math.Abs(job.Freq).ToString());
                timerDelegate = new TimerCallback(job.Export);
                timer = new Timer(timerDelegate, job.Config, job.DueTime, freq);
            }
        }

        [TestMethod]
        public async Task FullTest()
        {
            var configCS = ConfigurationManager.ConnectionStrings["CS_FarmaticDB"];
            if (null == configCS)
                throw new Exception("No app configuration found");
            if (!int.TryParse(ConfigurationManager.AppSettings["DaysToResend"], out int daysResend))
                throw new Exception($"DaysToResend config was not found or invalid, it must be a positive int");
            
            _config = new JobConfig
            {
                FarmaticConnectionString = configCS.ConnectionString,
                ProviderConnectionString = configCS.ProviderName,
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

            // try webApi retrieve lastrecord to limit request
            Stopwatch timerTokenRefresh = new Stopwatch();
            timerTokenRefresh.Start();
            APIMethods api = new APIMethods(_config);
            api.SetToken();
            if (api.TokenData == null)
                throw new System.Exception($"Could not obtain token from WebAPI, endpoint root was {_config.APIEndpoint}");

            // 2. Get 'last export date' (if I can add column: ExportDate, if not fallback to use FechaVenta) data from webAPI
            var _lastExportDate = api.GetLastExportInfo();
            DateTime searchFrom = DateTime.Now;

            // 3. Get all records from a day before 'last export date' to now with SQL in pages of 20 records each 
            if (_lastExportDate.Status == System.Net.HttpStatusCode.OK)
            {
                // searchFrom = searchFrom.AddDays(-1);
                var result = Math.Ceiling(DateTime.Now.Subtract(_lastExportDate.DateLastVenta).TotalDays);
                if (_config.DaysToResend < result)
                {
                    daysResend = int.Parse(Math.Ceiling(result).ToString());
                    Debug.WriteLine($"Getting from {daysResend} days ago: {DateTime.Now.AddDays(daysResend * -1).ToString("yyyy-MM-dd")}.");
                }
                else
                {
                    Debug.WriteLine($"Getting from {_config.DaysToResend} days ago: {DateTime.Now.AddDays(_config.DaysToResend * -1).ToString("yyyy-MM-dd")}.");
                }
            }
            else if (_lastExportDate.Status == System.Net.HttpStatusCode.NotFound)
            {
                // no hay registros, configurar para que envie todo (5 años)
                var dOld = DateTime.Now.AddYears(-5);
                var result = DateTime.Now.Subtract(dOld);
                Debug.WriteLine($"Getting all currrent records from {dOld.ToString("yyyy-MM-dd")}");
                daysResend = int.Parse(Math.Ceiling(result.TotalDays).ToString());
            }

            DALMethods dal = new DALMethods(_config);
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
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    //var value = reader["IdentificadorVenta"];

                    // conversion to DTO
                    if (VentaDTO.TryFromDBRecord(reader, out VentaDTO venta))
                    {
                        // convertir a JSON (WebAPI will receive this)
                        Debug.WriteLine(venta.ToString());
                        var result = api.PostVenta(venta);
                        if (!result)
                        {
                            var msg = $"ERROR enviando IdVenta {reader["IdentificadorVenta"]}";
                            Debug.WriteLine(msg);
                            throw new Exception(msg);                        
                        }

                        if (timerTokenRefresh.Elapsed.TotalMinutes > 4)
                        {
                            //refresh token
                            Debug.WriteLine("Refreshing api token");
                            api.SetToken();
                            if (api.TokenData == null)
                                throw new System.Exception($"Could not refresh token from WebAPI, endpoint root was {_config.APIEndpoint}");
                            else
                                timerTokenRefresh.Restart();
                        }
                    }
                    else
                    {
                        var msg = $"ERROR de conversion a DTO en IdVenta {reader["IdentificadorVenta"]}";
                        Debug.WriteLine(msg);
                        throw new Exception(msg);
                    }

                }
            }
            timer.Stop();

            Debug.WriteLine($"Processing time: {timer.Elapsed.TotalSeconds} secs");
        }

    }
}