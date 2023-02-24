using static CRPC.Common.Contracts;

namespace CRPC.Server
{
    public class DataService : IDataService
    {
        public async ValueTask<DataResponse> GetCustomDataAsync(CustomSizeDataRequest request)
        {
            return await Task.FromResult(new DataResponse()
            {
                Sender = Environment.MachineName.ToString(),
                Data = GenerateRandomAlphanumericString(request.Size)
            });
        }

        private static string GenerateRandomAlphanumericString(int? length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length ?? 10).Select(s => s[random.Next(s.Length)]).ToArray());

            return randomString;
        }
    }
}
