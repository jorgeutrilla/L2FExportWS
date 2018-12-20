using Business;
using Business.LocalModel;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Helpers
{
    public class APIMethods
    {
        private JobConfig _config;
        public Token TokenData { get; private set; }
        public APIMethods(JobConfig config)
        {
            _config = config;
        }

        public void SetToken()
        {
            TokenData = null;
            var client = new RestClient($"{_config.APIEndpoint}{_config.JWTAuthRoute}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined", $"username={_config.APIUser}&password={_config.APIPwd}&grant_type=password", ParameterType.RequestBody);

            var response = client.Execute<Token>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                TokenData = response.Data;
            else
                throw new Exception($"ERROR ({response.StatusCode}): {response.ErrorMessage}: {response.Content}");
            
        }


        public GetLastResult GetLastExportInfo()
        {
            var client = new RestClient($"{_config.APIEndpoint}{_config.APIGetVentaData}");
            var getLastRequest = new RestRequest(Method.GET);
            getLastRequest.AddHeader("cache-control", "no-cache");
            if (null == TokenData || string.IsNullOrEmpty(TokenData.access_token))
                throw new Exception("Token is null");

            //TODO: check token validity and refresh if need be

            getLastRequest.AddHeader("Authorization", $"Bearer {TokenData.access_token}");
            IRestResponse response = client.Execute(getLastRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No last one
                return new GetLastResult
                {
                    Status = System.Net.HttpStatusCode.NotFound,
                    DateLastVenta = DateTime.Now
                };
                // winserv should start retrieving 
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // something was returned
                var lastOne = JsonConvert.DeserializeObject<VentaDTO>(response.Content);
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
                throw new Exception($"{response.ErrorMessage}: {response.Content}");
            }
        }

        internal bool PostVenta(VentaDTO venta)
        {


            if (null == TokenData || string.IsNullOrEmpty(TokenData.access_token))
                throw new Exception("Token is null");

            //TODO: check token validity and refresh if need be
            var client = new RestClient($"{_config.APIEndpoint}{_config.APIPostVentaData}");
            client.AddDefaultHeader("content-type", "application/json");
            var postVenta = new RestRequest(Method.POST);
            postVenta.AddHeader("Authorization", $"Bearer {TokenData.access_token}");
            postVenta.AddJsonBody(venta);

            //postVenta.AddHeader("cache-control", "no-cache");
            //postVenta.AddHeader("content-type", "application/json");            
            //postVenta.AddParameter("venta", ventaJson, "application/json", ParameterType.RequestBody);
            IRestResponse response = client.Execute(postVenta);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // success - creation
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // success - update
                return true;
            }
            else
            {
                // some error - error management
                return false;
            }
        }
    }
}
