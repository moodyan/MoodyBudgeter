using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.RestRequester
{
    public interface IRestRequester
    {
        string BaseUrl { get; set; }
        string AuthorizationHeaderValue { get; set; }
        string ApiKey { get; set; }
        string RequestContentType { get; set; }
        HttpStatusCode StatusCode { get; set; }

        Task<T> MakeRequest<T>(string path, HttpMethod method, object body, Dictionary<string, string> customHeaders = null);

        Task<string> MakeRequest(string path, HttpMethod method, object body, Dictionary<string, string> customHeaders = null);
    }
}
