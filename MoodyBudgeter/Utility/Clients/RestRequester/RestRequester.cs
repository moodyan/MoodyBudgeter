using MoodyBudgeter.Models.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.RestRequester
{
    public class RestRequester : IRestRequester
    {
        public string BaseUrl { get; set; }
        public string AuthorizationHeaderValue { get; set; }
        public string ApiKey { get; set; }
        public string RequestContentType { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        private readonly HttpClient Client;

        public RestRequester(HttpClient client)
        {
            Client = client;
            RequestContentType = "application/json";
        }

        public async Task<T> MakeRequest<T>(string path, HttpMethod method, object body, Dictionary<string, string> customHeaders = null)
        {
            string responseContent = await MakeRequest(path, method, body, customHeaders);

            try
            {
                T responseData = JsonConvert.DeserializeObject<T>(responseContent);

                return responseData;
            }
            catch (Exception ex)
            {
                throw new CallerException(ex.ToString());
            }
        }

        public async Task<string> MakeRequest(string path, HttpMethod method, object body, Dictionary<string, string> customHeaders = null)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(method, BaseUrl + path))
            {
                if (method != HttpMethod.Get && body != null)
                {
                    request.Content = GetBody(body);
                }

                if (!string.IsNullOrEmpty(AuthorizationHeaderValue))
                {
                    request.Headers.Add(HttpRequestHeader.Authorization.ToString(), AuthorizationHeaderValue);
                }

                if (!string.IsNullOrEmpty(ApiKey))
                {
                    request.Headers.Add("x-api-key", ApiKey);
                }

                if (customHeaders != null)
                {
                    foreach (var header in customHeaders)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage response = await Client.SendAsync(request);

                StatusCode = response.StatusCode;

                string responseContent = await response.Content.ReadAsStringAsync();

                if (StatusCode != HttpStatusCode.OK)
                {
                    HandleError(responseContent, response.StatusCode);
                }

                return responseContent;
            }
        }

        private HttpContent GetBody(object body)
        {
            switch (RequestContentType)
            {
                case "application/json":
                    return new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                case "application/x-www-form-urlencoded":
                    return GetUrlEncodedContent(body);
                case "text/json":
                    return new StringContent((string)body, Encoding.UTF8, "text/json");
                default:
                    throw new NotSupportedException("ContentType is not supported");
            }
        }

        private FormUrlEncodedContent GetUrlEncodedContent(object body)
        {
            var properties = from p in body.GetType().GetProperties()
                             where p.GetValue(body, null) != null
                             select new KeyValuePair<string, string>(p.Name, p.GetValue(body, null).ToString());

            return new FormUrlEncodedContent(properties);
        }

        private void HandleError(string responseContent, HttpStatusCode statusCode)
        {
            ExceptionResponse exceptionResponse;

            try
            {
                exceptionResponse = JsonConvert.DeserializeObject<ExceptionResponse>(responseContent);
            }
            catch (Exception ex)
            {
                throw new BudgeterException("Internal response does not conform to ExceptionResponse", ex);
            }

            if (statusCode == HttpStatusCode.NotFound)
            {
                throw new BudgeterException("Resource not found");
            }

            if (exceptionResponse == null)
            {
                throw new BudgeterException("Internal response empty");
            }

            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new CallerException(exceptionResponse.Message, exceptionResponse.Exception);
                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException(exceptionResponse.Message, exceptionResponse.Exception);
                case HttpStatusCode.Forbidden:
                    throw new UnauthorizedAccessException(exceptionResponse.Message, exceptionResponse.Exception);
                case HttpStatusCode.Conflict:
                    throw new CallerException(exceptionResponse.Message, exceptionResponse.Exception);
                default:
                    throw new BudgeterException(exceptionResponse.Message, exceptionResponse.Exception);
            }
        }
    }
}
