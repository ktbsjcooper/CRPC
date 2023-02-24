using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CRPC.Common
{
    public class Contracts
    {
        [ServiceContract]
        public interface IDataService
        {
            ValueTask<DataResponse> GetCustomDataAsync(CustomSizeDataRequest request);
        }

        [DataContract]
        public class DataResponse
        {
            [DataMember(Order = 1)]
            public string? Sender { get; set; }

            [DataMember(Order = 2)]
            public string? Data { get; set; }
        }

        [DataContract]
        public class CustomSizeDataRequest
        {
            [DataMember(Order = 1)]
            public int? Size { get; set; }
        }
    }
}
