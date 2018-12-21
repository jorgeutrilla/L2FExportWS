using API.DTOs;
using Business;
using Business.LocalModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestWebAPI
    {

        public JobConfig Config { get; set; }

        private Token _token;

        [TestInitialize]
        public void Init()
        {
            Config = new JobConfig
            {
                APIEndpoint = "http://localhost:53986",
                APIUser = "soporte@makesoft.es",
                APIPwd = "Qwerty.123$",
                JWTAuthRoute = "/token",
                APIGetVentaData = "/api/ventas",
                APIPostVentaData = "/api/ventas/single",
                APIPostVentaDataRange = "/api/ventas/bloque",
                DaysToResend = 20,
                APICodUsuario = "ABCDS",                
                UseAPIRangeMethod = false
            };


            
        }

        [TestMethod]
        public void TestGetToken()
        {
            ObtainToken();
            Assert.IsTrue(_token != null && !string.IsNullOrEmpty(_token.access_token));
        }


        [TestMethod]
        public void TestWebApiPost()
        {
            // 1.1 Get webAPI credentials and get token
            ObtainToken();
            if (_token == null)
                throw new System.Exception($"Could not obtain token from WebAPI, endpoint root was {Config.APIEndpoint}");

            // 2. Get 'last export date' (if I can add column: ExportDate, if not fallback to use FechaVenta) data from webAPI
            var _lastExportDate = GetLastExport();
            DateTime searchFrom = DateTime.Now;
            
            // 3. Get all records from a day before 'last export date' to now with SQL in pages of 20 records each 
            if (_lastExportDate.Status == System.Net.HttpStatusCode.NotFound)
            {
                searchFrom = searchFrom.AddDays(Config.DaysToResend * -1);
            } else if (_lastExportDate.Status == System.Net.HttpStatusCode.OK)
            {
                searchFrom = searchFrom.AddDays(-1);
            }

            Debug.WriteLine($"Windows service should retrieve from {searchFrom}");
            Assert.IsTrue(DateTime.Now.CompareTo(searchFrom) > 0);

            // 4. Send page Loop

            // 4.1 SPL: if OK && ExportDate -> Set Export Date on each of the 20 records sent
            // 4.2 SPL: if !OK -> Store error log info on a var for notifications process

        }

        private GetLastResult GetLastExport()
        {
            var client = new RestClient($"{Config.APIEndpoint}{Config.APIGetVentaData}");
            var getLastRequest = new RestRequest(Method.GET);
            getLastRequest.AddHeader("cache-control", "no-cache");
            if (null == _token)
                throw new Exception("Token is null");
            getLastRequest.AddHeader("Authorization", $"Bearer {_token.access_token}");
            IRestResponse response = client.Execute(getLastRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No last one
                Debug.WriteLine($"No LastOne found. Service should retrieve");
                return new GetLastResult{
                    Status = System.Net.HttpStatusCode.NotFound,
                    DateLastVenta = DateTime.Now
                };
                // winserv should start retrieving 
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // something was returned
                var lastOne = JsonConvert.DeserializeObject<Business.LocalModel.VentaDTO>(response.Content);
                Debug.WriteLine($"LastOne found: {lastOne.IdentificadorVenta}-{lastOne.IdLinea}:{lastOne.FechaVenta}");
                return new GetLastResult
                {
                    Status = response.StatusCode,
                    DateLastVenta = lastOne.FechaVenta
                };
                // extract FechaVenta and retrieve from it minus x days
            }
            else
            {
                // some error - error management
                Debug.WriteLine($"ERROR: {response.ErrorMessage}");
                throw new Exception($"{response.ErrorMessage}: {response.Content}");
            }
        }

        private void ObtainToken()
        {
            var client = new RestClient($"{Config.APIEndpoint}{Config.JWTAuthRoute}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined", $"username={Config.APIUser}&password={Config.APIPwd}&grant_type=password", ParameterType.RequestBody);

            var response = client.Execute<Token>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                _token = response.Data;
            else
            {
                Debug.WriteLine($"ERROR({response.StatusCode}): {response.ErrorMessage}: {response.Content}");
            }
        }
    }
}
