using System.ComponentModel.DataAnnotations;

namespace GrpcTestWebApi.Common.Context
{
    public class CbrRate
    {
        [Key]
        public string Code { get; set; }

        public string Title { get; set; }

        public int Count { get; set; }

        public double Rate { get; set; }
    }
}
