using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GrpcTestWebApi.Common
{
    public static class SoapHelper
    {
        public static async Task<string> CallWebService(
            string webWebServiceUrl,
            string webServiceNamespace,
            string methodVerb,
            string methodName,
            Dictionary<string, string> parameters)
        {
            const string soapTemplate =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:{0}=""{2}"">
<soapenv:Header />
<soapenv:Body>
    <{0}:{1}>
        {3}
    </{0}:{1}>
</soapenv:Body>
</soapenv:Envelope>";

            var req = (HttpWebRequest) WebRequest.Create(webWebServiceUrl);
            req.AllowAutoRedirect = false;
            req.ContentType = "text/xml; charset=utf-8";
            req.Method = "POST";

            string parametersText;

            if (parameters != null && parameters.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var oneParameter in parameters)
                {
                    sb.AppendFormat("<{0}:{1}>{2}</{0}:{1}>", methodVerb, oneParameter.Key, oneParameter.Value);
                }

                parametersText = sb.ToString();
            }
            else
            {
                parametersText = string.Empty;
            }

            string soapText = string.Format(
                soapTemplate,
                methodVerb,
                methodName,
                webServiceNamespace,
                parametersText);

            using (var stm = await req.GetRequestStreamAsync())
            {
                using (var stmw = new StreamWriter(stm))
                {
                    stmw.Write(soapText);
                }
            }

            string responseText = null;

            using (var response = (HttpWebResponse) req.GetResponseAsync().Result)
            {
                var responseHttpStatusCode = response.StatusCode;

                if (responseHttpStatusCode == HttpStatusCode.OK)
                {
                    int contentLength = (int) response.ContentLength;

                    if (contentLength > 0)
                    {
                        int readBytes = 0;
                        int bytesToRead = contentLength;
                        byte[] resultBytes = new byte[contentLength];

                        using (var responseStream = response.GetResponseStream())
                        {
                            while (bytesToRead > 0)
                            {
                                int actualBytesRead = responseStream.Read(resultBytes, readBytes, bytesToRead);

                                if (actualBytesRead == 0)
                                {
                                    break;
                                }

                                readBytes += actualBytesRead;
                                bytesToRead -= actualBytesRead;
                            }

                            responseText = Encoding.UTF8.GetString(resultBytes);
                        }
                    }
                }
            }

            return responseText;
        }
    }
}