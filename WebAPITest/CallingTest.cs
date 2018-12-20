using API.DTOs;
using API.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WebAPITest.Tools;

namespace WebAPITest
{
    [TestClass]
    public class CallingTest
    {
        private readonly string _api_root_endpoint = "http://localhost:53986";
        private readonly string _token_endpoint = "/token";
        private readonly string _ventas_endpoint = "/api/ventas/simple";
        private readonly string _ventas_endpoint_bloque = "/api/ventas/bloque";

        private Token _token = null;

        [TestInitialize]
        public async Task Init()
        {
            await TestCallToken();
        }

        [TestMethod]
        public async Task TestCallToken()
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined", "username=soporte%40makesoft.es&password=Qwerty.123%24&grant_type=password&undefined=", ParameterType.RequestBody);

            _token = await GetToken(request);
            Debug.WriteLine($"Token: {_token.access_token}");
        }

        [TestMethod]
        public async Task TestGetLast()
        {
            var client = new RestClient($"{_api_root_endpoint}{_ventas_endpoint}");
            var getLastRequest = new RestRequest(Method.GET);
            getLastRequest.AddHeader("cache-control", "no-cache");
            if (null == _token)
                await TestCallToken();
            getLastRequest.AddHeader("Authorization", $"Bearer {_token.access_token}");
            IRestResponse response = client.Execute(getLastRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No last one
                Debug.WriteLine($"No LastOne found. Service should retrieve");

                // winserv should start retrieving 
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // something was returned
                var lastOne = JsonConvert.DeserializeObject<VentaDTO>(response.Content);
                Debug.WriteLine($"LastOne found: {lastOne.IdentificadorVenta}-{lastOne.IdLinea}:{lastOne.FechaVenta}");

                // extract FechaVenta and retrieve from it minus x days
            }
            else
            {
                // some error - error management
                Debug.WriteLine($"ERROR: {response.ErrorMessage}");

            }
        }


        [TestMethod]
        public async Task GetVenta()
        {
            int idVenta = 0;
            var client = new RestClient($"{_api_root_endpoint}{_ventas_endpoint}/{idVenta}");
            var getVentaAsVentaListObject = new RestRequest(Method.GET);
            getVentaAsVentaListObject.AddHeader("cache-control", "no-cache");
            if (null == _token)
                await TestCallToken();
            getVentaAsVentaListObject.AddHeader("Authorization", $"Bearer {_token.access_token}");
            IRestResponse response = client.Execute(getVentaAsVentaListObject);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Not found
                Debug.WriteLine($"Not found.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // something was returned
                var venta = JsonConvert.DeserializeObject<VentaLineasDTO>(response.Content);
                Debug.WriteLine($"Venta: {venta.IdVenta}:{venta.FechaVenta}:{venta.CodLaboratorio}");
                foreach (var linea in venta.Lineas)
                {
                    Debug.WriteLine($"Linea: {linea.IdLinea}:{linea.CodProducto}-{linea.DescProducto}:{linea.PVP}");
                }

                // extract FechaVenta and retrieve from it minus x days
            }
            else
            {
                // some error - error management
                Debug.WriteLine($"ERROR: {response.ErrorMessage}");

            }
        }

        [TestMethod]
        public async Task TestPostVenta()
        {
            VentaDTO ventaToPost = new VentaDTO
            {
                CantidadVendida = 10,
                CodLaboratorio = "E0644",
                CodProducto = "874221",
                DescProducto = "OMEPRAZOL BEXAL 20 MG 28 CAPSULAS GASTRORRESISTE",
                EsGenerico = true,
                FechaVenta = DateTime.Parse("2018-12-14T10:42:31.6845383+01:00"),
                IdentificadorVenta = 1,
                IdLinea = 0,
                LoteOptimo = 42,
                NombreLaboratorio = "SANDOZ FARMACEUTICA S.A.",
                PVP = 16,
                StockActual = 150,
                StockMaximo = 1000,
                StockMinimo = 1,
                TipoVenta = "R"
            };
            string ventaToPostJson = JsonConvert.SerializeObject(ventaToPost);
            var client = new RestClient($"{_api_root_endpoint}{_ventas_endpoint}");
            var postVenta = new RestRequest(Method.POST);
            postVenta.AddHeader("cache-control", "no-cache");
            if (null == _token)
                await TestCallToken();
            postVenta.AddHeader("Authorization", $"Bearer {_token.access_token}");
            postVenta.AddHeader("Content-Type", "application/json");
            postVenta.AddParameter("venta", ventaToPostJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(postVenta);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // success - creation
                Debug.WriteLine($"Created.");
                var venta = JsonConvert.DeserializeObject<Venta>(response.Content);
                Debug.WriteLine($"[{venta.IdentificadorVenta}:{venta.IdLinea}] - {venta.FechaRecibido}.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // success - update
                Debug.WriteLine($"Updated.");
                var venta = JsonConvert.DeserializeObject<Venta>(response.Content);
                Debug.WriteLine($"[{venta.IdentificadorVenta}:{venta.IdLinea}] - {venta.FechaRecibido}.");
            }
            else
            {
                // some error - error management
                Debug.WriteLine($"ERROR: {response.ErrorMessage}");
            }

        }


        [TestMethod]
        public async Task TestPostListVenta()
        {
            var ventaList = new List<VentaDTO>
            {
                new VentaDTO
                {
                    CantidadVendida = 10,
                    CodLaboratorio = "E0644",
                    CodProducto = "874221",
                    DescProducto = "OMEPRAZOL BEXAL 20 MG 28 CAPSULAS GASTRORRESISTE",
                    EsGenerico = true,
                    FechaVenta = DateTime.Parse("2018-12-14T10:42:31.6845383+01:00"),
                    IdentificadorVenta = 0,
                    IdLinea = 1,
                    LoteOptimo = 42,
                    NombreLaboratorio = "SANDOZ FARMACEUTICA S.A.",
                    PVP = 16,
                    StockActual = 150,
                    StockMaximo = 1000,
                    StockMinimo = 1,
                    TipoVenta = "R"
                },
                new VentaDTO
                {
                    CantidadVendida = 10,
                    CodLaboratorio = "E0644",
                    CodProducto = "874221",
                    DescProducto = "OMEPRAZOL BEXAL 20 MG 28 CAPSULAS GASTRORRESISTE",
                    EsGenerico = true,
                    FechaVenta = DateTime.Parse("2018-12-14T10:42:31.6845383+01:00"),
                    IdentificadorVenta = 0,
                    IdLinea = 2,
                    LoteOptimo = 42,
                    NombreLaboratorio = "SANDOZ FARMACEUTICA S.A.",
                    PVP = 16,
                    StockActual = 150,
                    StockMaximo = 1000,
                    StockMinimo = 1,
                    TipoVenta = "R"
                },
                new VentaDTO
                {
                    CantidadVendida = 10,
                    CodLaboratorio = "E0644",
                    CodProducto = "874221",
                    DescProducto = "OMEPRAZOL BEXAL 20 MG 28 CAPSULAS GASTRORRESISTE",
                    EsGenerico = true,
                    FechaVenta = DateTime.Parse("2018-12-14T10:42:31.6845383+01:00"),
                    IdentificadorVenta = 0,
                    IdLinea = 3,
                    LoteOptimo = 42,
                    NombreLaboratorio = "SANDOZ FARMACEUTICA S.A.",
                    PVP = 16,
                    StockActual = 150,
                    StockMaximo = 1000,
                    StockMinimo = 1,
                    TipoVenta = "R"
                },
                new VentaDTO
                {
                    CantidadVendida = 10,
                    CodLaboratorio = "E0644",
                    CodProducto = "874221",
                    DescProducto = "OMEPRAZOL BEXAL 20 MG 28 CAPSULAS GASTRORRESISTE",
                    EsGenerico = true,
                    FechaVenta = DateTime.Parse("2018-12-14T10:42:31.6845383+01:00"),
                    IdentificadorVenta = 0,
                    IdLinea = 4,
                    LoteOptimo = 42,
                    NombreLaboratorio = "SANDOZ FARMACEUTICA S.A.",
                    PVP = 16,
                    StockActual = 150,
                    StockMaximo = 1000,
                    StockMinimo = 1,
                    TipoVenta = "R"
                },
            };

            var ventaListJson = new
            {
                ventas = ventaList
            };

            string ventaToPostJson = JsonConvert.SerializeObject(ventaListJson);
            var client = new RestClient($"{_api_root_endpoint}{_ventas_endpoint_bloque}");
            var postVenta = new RestRequest(Method.POST);
            postVenta.AddHeader("cache-control", "no-cache");
            if (null == _token)
                await TestCallToken();

            postVenta.AddHeader("Authorization", $"Bearer {_token.access_token}");
            postVenta.AddHeader("Content-Type", "application/json");
            postVenta.AddParameter("ventas", ventaToPostJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(postVenta);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // success - creation
                Debug.WriteLine($"Created.");
                var result = JsonConvert.DeserializeObject<ListProcessResult>(response.Content);
                Debug.WriteLine($"Enviados {ventaList.Count} -> Creados: {result.Creations}; Actualizados {result.Updates}.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // success - update
                Debug.WriteLine($"Updated.");
                var result = JsonConvert.DeserializeObject<ListProcessResult>(response.Content);
                Debug.WriteLine($"Enviados {ventaList.Count} -> Creados: {result.Creations}; Actualizados {result.Updates}.");
            }
            else
            {
                // some error - error management
                Debug.WriteLine($"ERROR: {response.ErrorMessage}");
            }

        }


        public async Task<Token> GetToken(RestRequest request)
        {
            var client = new RestClient($"{_api_root_endpoint}{_token_endpoint}");
            var cancellationTokenSource = new CancellationTokenSource();
            var response = await client.ExecuteTaskAsync<Token>(request, cancellationTokenSource.Token);
            return response.Data;
        }
    }
}
