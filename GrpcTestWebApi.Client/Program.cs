using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcTestWebApi.Common;
using GrpcTestWebApi.Service;
using Newtonsoft.Json;

namespace GrpcTestWebApi.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await DoAction();
        }

        private static async Task DoAction()
        {
            Console.WriteLine("Input filtering query:");
            var query = Console.ReadLine();
            try
            {
                //gRPC
                var begin = DateTime.Now;

                var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = 100 * 1024 * 1024,
                    MaxSendMessageSize = 100 * 1024 * 1024
                });
                var gRpcClient = new ExchangeRates.ExchangeRatesClient(channel);
                var res = await gRpcClient.GetRatesAsync(new RequestInfo
                {
                    Query = query
                });

                var end = DateTime.Now;

                Console.WriteLine($"gRPC processing time (ms): {(end - begin).TotalMilliseconds}");

                foreach (var rate in res.Rates)
                {
                    Console.WriteLine($"{rate.Code}, {rate.Title}, {rate.Rate} for {rate.Count}");
                }

                //JSON data
                begin = DateTime.Now;

                var httpClient = new HttpClient();

                string url = "https://localhost:44343/api/exchangerates";
                if (!string.IsNullOrEmpty(query))
                {
                    url = $"{url}?${query}";
                }

                var response = await httpClient.GetAsync(url);
                var data = await response.Content.ReadAsStringAsync();
                var rates = JsonConvert.DeserializeObject<IEnumerable<DtoRate>>(data);

                end = DateTime.Now;

                Console.WriteLine($"JSON processing time (ms): {(end - begin).TotalMilliseconds}");

                foreach (var rate in rates)
                {
                    Console.WriteLine($"{rate.Code}, {rate.Title}, {rate.Rate} for {rate.Count}");
                }

                Console.WriteLine("Continue (Y/N):");

                var yes = Console.ReadLine();
                if (string.Equals(yes, "y", StringComparison.InvariantCultureIgnoreCase))
                {
                    await DoAction();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Incorrect input - {(ex.InnerException ?? ex).Message}");
            }
        }
    }
}
