using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using GrpcTestWebApi.Common;
using GrpcTestWebApi.Common.Context;

namespace GrpcTestWebApi.WebApi.Services
{
    internal class RatesService : IRatesService
    {
        private const string CBR_DAILY_SERIVCE_URL = "http://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
        private const string NAMESPACE = "http://web.cbr.ru/";
        private const string VERB = "web";

        private readonly DataContext _context;

        public RatesService(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<DtoRate>> GetRatesAsync()
        {
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

            var query = _context.Rates.Select(i => new DtoRate
            {
                Code = i.Code,
                Title = i.Title,
                Count = i.Count,
                Rate = i.Rate
            });

            return query;
            //return query.ToList().AsQueryable();
        }
    }
}
