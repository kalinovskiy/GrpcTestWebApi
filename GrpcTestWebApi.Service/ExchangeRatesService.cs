using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcTestWebApi.Common;
using GrpcTestWebApi.Common.Context;
using Newtonsoft.Json;
using OdataToEntity;
using OdataToEntity.EfCore;

namespace GrpcTestWebApi.Service
{
    public class ExchangeRatesService : ExchangeRates.ExchangeRatesBase
    {
        private readonly DataContext _context;
        private const string CBR_DAILY_SERIVCE_URL = "http://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
        private const string NAMESPACE = "http://web.cbr.ru/";
        private const string VERB = "web";

        public ExchangeRatesService(DataContext context)
        {
            _context = context;
        }

        public override async Task<ResponseInfo> GetRates(RequestInfo request, ServerCallContext context)
        {
            var response = new ResponseInfo();

            var res = await SoapHelper.CallWebService(
                CBR_DAILY_SERIVCE_URL,
                NAMESPACE,
                VERB,
                "GetCursOnDate",
                new Dictionary<string, string>
                {
                    { "On_date", DateTime.Now.ToString("yyyy-MM-dd") },
                });

            var doc = new XmlDocument();
            doc.LoadXml(res);

            var datas = doc.SelectNodes("//ValuteCursOnDate");

            _context.Rates.RemoveRange(_context.Rates);
            _context.SaveChanges();

            foreach (XmlElement data in datas)
            {
                try
                {
                    _context.Rates.Add(new CbrRate
                    {
                        Code = data.SelectSingleNode("VchCode").InnerText.Trim(),
                        Title = data.SelectSingleNode("Vname").InnerText.Trim(),
                        Count = short.Parse(data.SelectSingleNode("Vnom").InnerText),
                        Rate = double.Parse(data.SelectSingleNode("Vcurs").InnerText, CultureInfo.InvariantCulture),
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            _context.SaveChanges();

            var rates = await GetDataFromContextByODataFilterAsync<DataContext, IEnumerable<DtoRate>>(request.Query);

            foreach (var rate in rates)
            {
                var eRate = new ExchangeRate
                {
                    Code = new StringValue(),
                    Title = new StringValue(),
                    Count = new Int32Value(),
                    Rate = new DoubleValue()
                };

                if (!string.IsNullOrEmpty(rate.Code))
                {
                    eRate.Code.Value = rate.Code;
                }

                if (!string.IsNullOrEmpty(rate.Title))
                {
                    eRate.Title.Value = rate.Title;
                }

                if (rate.Count.HasValue)
                {
                    eRate.Count.Value = rate.Count.Value;
                }

                if (rate.Rate.HasValue)
                {
                    eRate.Rate.Value = rate.Rate.Value;
                }

                response.Rates.Add(eRate);
            }

            return response;
        }

        private async Task<TRes> GetDataFromContextByODataFilterAsync<TContext, TRes>(string query)
            where TContext : DataContext
        {
            var dataAdapter = new OeEfCoreDataAdapter<TContext>();
            var edmModel = dataAdapter.BuildEdmModel();
            var parser = new OeParser(new Uri("http://dummy"), edmModel);
            var uri = new Uri("http://dummy/Rates?$" + query);
            
            var response = new MemoryStream();
            await parser.ExecuteGetAsync(uri, OeRequestHeaders.JsonDefault, response, CancellationToken.None);
            
            var res = Encoding.UTF8.GetString(response.GetBuffer());

            var odataResult = JsonConvert.DeserializeObject<DtoODataResult<TRes>>(res);

            return odataResult.Value;
        }
    }
}
