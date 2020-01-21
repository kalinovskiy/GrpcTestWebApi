using Newtonsoft.Json;

namespace GrpcTestWebApi.Common
{
    public class DtoODataResult<T>
    {
        [JsonProperty("@odata.context")]
        public string Query { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}
