using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PrivateRepoAccessAPILib.Model;

namespace PrivateRepoAccessAPILib.Helper
{
    public class HttpHelper
    {
        /// <summary>
        /// The HTTP client time out minutes.
        /// </summary>
        private const int HttpClientTimeOutMinutes = 5;

        //private static ILog log = LogManager.GetLogger(typeof(HttpHelper));

        /// <summary>
        /// Sends the web request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="data">The data.</param>
        /// <param name="method">The method.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>
        /// Web request response.
        /// </returns>
        /// <exception cref="Exception">Web request exception.</exception>
        public static string SendWebRequest(string url, List<KeyValuePair<string, string>> headers, string data = null, string method = "GET", string contentType = "application/json")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            string response = string.Empty;
            string error = string.Empty;

            // log.InfoFormat("Put Request Data: {0}", data);
            try
            {
                req.Method = method;
                req.Timeout = 120000;
                req.ContentType = contentType;
                foreach (var header in headers)
                {
                    req.Headers[header.Key] = header.Value;
                }

                if (!string.IsNullOrWhiteSpace(data))
                {
                    byte[] sentData = Encoding.UTF8.GetBytes(data);
                    req.ContentLength = sentData.Length;

                    using (System.IO.Stream sendStream = req.GetRequestStream())
                    {
                        sendStream.Write(sentData, 0, sentData.Length);
                        sendStream.Close();
                    }
                }
                else
                {
                    req.ContentLength = 0;
                }

                System.Net.WebResponse res = req.GetResponse();
                System.IO.Stream receiveStream = res.GetResponseStream();
                using (System.IO.StreamReader sr = new
                System.IO.StreamReader(receiveStream, Encoding.UTF8))
                {
                    char[] read = new char[256];
                    int count = sr.Read(read, 0, 256);

                    while (count > 0)
                    {
                        string str = new string(read, 0, count);
                        response += str;
                        count = sr.Read(read, 0, 256);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                error = string.Format("HTTP_ERROR :: The second HttpWebRequest object has raised an Argument Exception as 'Connection' Property is set to 'Close' :: {0}", ex.Message);
                //log.Error(error, ex);
                throw ex;
            }
            catch (WebException ex)
            {
                error = string.Format("HTTP_ERROR :: WebException raised! :: {0}", ex.Message);
                if (ex.Response != null)
                {
                    Stream responseStream = ex.Response.GetResponseStream();
                    var responseString = new StreamReader(responseStream, Encoding.UTF8).ReadToEnd();

                    //log.ErrorFormat("Web exception while sending azure request : {0}", responseString);

                    throw new Exception(responseString, ex);
                }

                // log.Error(error, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                error = string.Format("HTTP_ERROR :: Exception raised! :: {0}", ex.Message);

                // log.Error(error, ex);
                throw ex;
            }

            // log.Info(response);
            Console.WriteLine(error);

            return response;
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="data">The data.</param>
        /// <param name="queryStringParams">The query string parameters.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>
        /// HttpResponse.
        /// </returns>
        /// <exception cref="Exception">Error occurred while communicating with gateway.</exception>
        public static async Task<HttpResponse> SendRequest(string accessToken, HttpMethod verb, string endpoint, string data = null, Dictionary<string, string> queryStringParams = null, string contentType = "application/json", Dictionary<string, string> headers = null, string type="devops")
        {
            HttpClient httpClient = new HttpClient();

            var queryString = BuildQueryString(queryStringParams);
            HttpRequestMessage request = new HttpRequestMessage(verb, endpoint + queryString);

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                if(type.Equals("git"))
                {
                    request.Headers.Add("User-Agent", "PostmanRuntime/7.26.1");
                }
            }

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            try
            {
                if (data != null)
                {
                    request.Content = new StringContent(data, Encoding.UTF8, contentType);
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    ResponseContentType = response.Content.Headers.ContentType.MediaType.ToString(),
                    RequestUrl = response.RequestMessage.RequestUri.ToString(),
                    RawResponse = await response.Content.ReadAsStringAsync(),
                    ByteResponse = await response.Content.ReadAsByteArrayAsync(),
                };
            }
            catch (Exception exc)
            {
                throw new Exception("Error occurred while communicating with gateway.", exc);
            }
        }

        /// <summary>Sends the request with basic authentication.</summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="data">The data.</param>
        /// <param name="queryStringParams">The query string parameters.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>HttpResponse.</returns>
        /// <exception cref="Exception">Error occurred while communicating with gateway.</exception>
        public static async Task<HttpResponse> SendRequestWithBasicAuthentication(string accessToken, HttpMethod verb, string endpoint, string data = null, Dictionary<string, string> queryStringParams = null, string contentType = null)
        {
            HttpClient httpClient = new HttpClient();

            var queryString = BuildQueryString(queryStringParams);
            HttpRequestMessage request = new HttpRequestMessage(verb, endpoint + queryString);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Add("Authorization", $"Basic {accessToken}");
            }

            try
            {
                if (data != null)
                {
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    ResponseContentType = response.Content.Headers.ContentType.MediaType.ToString(),
                    RequestUrl = response.RequestMessage.RequestUri.ToString(),
                    RawResponse = await response.Content.ReadAsStringAsync(),
                    ByteResponse = await response.Content.ReadAsByteArrayAsync(),
                };
            }
            catch (Exception exc)
            {
                throw new Exception("Error occurred while communicating with gateway.", exc);
            }
        }

        /// <summary>Sends the form URL encoded request.</summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="queryStringParams">The query string parameters.</param>
        /// <param name="formBodyParams">The form body parameters.</param>
        /// <returns>HttpResponse.</returns>
        public static async Task<HttpResponse> SendFormUrlEncodedRequest(string endpoint, Dictionary<string, string> queryStringParams, List<KeyValuePair<string, string>> formBodyParams)
        {
            HttpClient httpClient = new HttpClient();
            var queryString = queryStringParams != null && queryStringParams.Count > 0 ? BuildQueryString(queryStringParams) : string.Empty;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint + queryString)
                {
                    Content = new FormUrlEncodedContent(formBodyParams),
                };

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    RequestUrl = response.RequestMessage.RequestUri.ToString(),
                    RawResponse = await response.Content.ReadAsStringAsync(),
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>Sends the request with basic authentication.</summary>
        /// <param name="verb">The verb.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="data">The data.</param>
        /// <param name="queryStringParams">The query string parameters.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>SendRequestWithBasicAuthentication.</returns>
        /// <exception cref="Exception">Error occurred while communicating with gateway.</exception>
        public static async Task<HttpResponse> SendRequestWithBasicAuthentication(HttpMethod verb, string endpoint, string username, string password, string data = null, Dictionary<string, string> queryStringParams = null, string contentType = null)
        {
            HttpClient httpClient = new HttpClient();

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{username}:{password}");
            string val = System.Convert.ToBase64String(plainTextBytes);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);

            var queryString = BuildQueryString(queryStringParams);
            HttpRequestMessage request = new HttpRequestMessage(verb, endpoint + queryString);

            try
            {
                if (data != null)
                {
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    RequestUrl = response.RequestMessage.RequestUri.ToString(),
                    RawResponse = await response.Content.ReadAsStringAsync(),
                };
            }
            catch (Exception exc)
            {
                throw new Exception("Error occurred while communicating with gateway.", exc);
            }
        }

        /// <summary>
        /// Invokes the rest method.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="type">The type.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>HttpResponseMessage.</returns>
        public static HttpResponseMessage InvokeRestMethod(string url, HttpMethod type, string accessToken, string data, string contentType = "application/json")
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(HttpClientTimeOutMinutes);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = new HttpResponseMessage();

            var datacontent = new StringContent(string.Empty);
            if (contentType == "application/x-www-form-urlencoded" && type == HttpMethod.Post)
            {
                datacontent = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            }
            else
            {
                datacontent = new StringContent(data, Encoding.UTF8, contentType);
            }

            if (type == HttpMethod.Get)
            {
                response = client.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else if (type == HttpMethod.Post)
            {
                response = client.PostAsync(url, datacontent).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else if (type == HttpMethod.Put)
            {
                response = client.PutAsync(url, datacontent).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return response;
        }

        /// <summary>
        /// Invokes the rest method synchronize.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="type">The type.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> InvokeRestMethodAsync(string url, HttpMethod type, string accessToken, string data, string contentType = "application/json")
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(HttpClientTimeOutMinutes);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = new HttpResponseMessage();

            var datacontent = new StringContent(string.Empty);
            if (contentType == "application/x-www-form-urlencoded" && type == HttpMethod.Post)
            {
                datacontent = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
            }
            else
            {
                datacontent = new StringContent(data, Encoding.UTF8, contentType);
            }

            if (type == HttpMethod.Get)
            {
                response = await client.GetAsync(url);
            }
            else if (type == HttpMethod.Post)
            {
                response = await client.PostAsync(url, datacontent);
            }
            else if (type == HttpMethod.Put)
            {
                response = await client.PutAsync(url, datacontent);
            }

            return response;
        }

        /// <summary>Sends the request with access token.</summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="data">The data.</param>
        /// <param name="queryStringParams">The query string parameters.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>SendRequestWithAccessToken.</returns>
        /// <exception cref="Exception">Error occurred while communicating with gateway.</exception>
        public static async Task<HttpResponse> SendRequestWithAccessToken(string accessToken, HttpMethod verb, string endpoint, string data = null, Dictionary<string, string> queryStringParams = null, string contentType = null)
        {
            HttpClient httpClient = new HttpClient();

            var queryString = BuildQueryString(queryStringParams);
            HttpRequestMessage request = new HttpRequestMessage(verb, endpoint + queryString);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Add("Authorization", accessToken);
            }

            try
            {
                if (data != null)
                {
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);
                return new HttpResponse
                {
                    StatusCode = response.StatusCode,
                    RequestUrl = response.RequestMessage.RequestUri.ToString(),
                    RawResponse = await response.Content.ReadAsStringAsync(),
                };
            }
            catch (Exception exc)
            {
                throw new Exception("Error occurred while communicating with gateway.", exc);
            }
        }

        private static string BuildQueryString(Dictionary<string, string> queryStringParams)
        {
            if (queryStringParams == null)
            {
                return string.Empty;
            }

            return string.Format("?{0}", string.Join("&", queryStringParams.Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))));
        }
    }
}
