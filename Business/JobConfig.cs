using System;
using System.Configuration;
using System.Text;

namespace Business
{
    public class JobConfig
    {
        public string FarmaticConnectionString { get; set; }
        public string APIEndpoint { get; set; }
        public string APIUser { get; set; }
        public string APIPwd { get; set; }
        public string JWTAuthRoute { get; set; }
        public string APIGetVentaData { get; set; }
        public string APIPostVentaData { get; set; }
        public string APIPostVentaDataRange { get; set; }
        public string ProviderConnectionString { get; set; }
        public int DaysToResend { get; set; } = 1;
        public string APICodUsuario { get; set; }

        public override string ToString()
        {
            string retVal = "";
            StringBuilder strB = new StringBuilder();
            strB.AppendLine($"Farmatic CS {FarmaticConnectionString}");
            strB.AppendLine($"FCS DBB Prov: {ProviderConnectionString}");
            strB.AppendLine($"API Endpoint {APIEndpoint}");
            strB.AppendLine($"API Creds {APIUser}/******");
            strB.AppendLine($"API CodUsuario {APICodUsuario}");
            strB.AppendLine($"API Auth Endpoint {APIEndpoint}/{JWTAuthRoute}");
            strB.AppendLine($"APIDataGetPostRoute {APIEndpoint}{APIGetVentaData}, {APIEndpoint}{APIPostVentaData}");
            strB.AppendLine($"DaysToResend {DaysToResend}");
            retVal = strB.ToString();
            return retVal;
        }
    }
}