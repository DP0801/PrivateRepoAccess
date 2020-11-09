using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using log4net;
using PrivateRepoAccessAPILib.Contracts;
using PrivateRepoAccessAPILib.Constants;
using PrivateRepoAccessAPILib.Helper;
using PrivateRepoAccessAPILib.Model;
using System.Data.SqlClient;
using System.Data;

namespace PrivateRepoAccessAPILib.Service
{
    public class AzureDevopsService : IRepositoryService
    {
        private readonly string devopsApiEndpoint;
        private static readonly ILog Log = LogManager.GetLogger(typeof(AzureDevopsService));
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public string organizationName;
        public string projectName;
        public string path;
        public string PATToken;

        public AzureDevopsService(string devopsApiEndpoint = "https://dev.azure.com")
        {
            this.devopsApiEndpoint = devopsApiEndpoint;
        }
        public PrivateAccessReponse GetFileContent()
        {
            PrivateAccessReponse privateAccessReponse = new PrivateAccessReponse()
            {
                ByteResponse = null,
            };
            try
            { 
                if (!string.IsNullOrEmpty(PATToken))
                {
                    string basicAuth = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PATToken)));
                    string endPointURL = string.Format("{0}/{1}/_apis/git/repositories?api-version=5.1", devopsApiEndpoint, organizationName);
                    var apiResponse = HttpHelper.SendRequestWithBasicAuthentication(basicAuth, HttpMethod.Get, endPointURL).GetAwaiter().GetResult();

                    if (apiResponse.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(apiResponse.RawResponse))
                    {
                        var parsedAppListResult = JsonDocument.Parse(apiResponse.RawResponse);
                        var repoResponse = JsonSerializer.Deserialize<List<Repository>>(parsedAppListResult?.RootElement.GetProperty("value").ToString(), options);

                        if (repoResponse.Count > 0)
                        {
                            Repository repoModel = repoResponse.Where(x => x.Name.ToLower().Equals(projectName.ToLower()) == true).FirstOrDefault();
                            if (repoModel != null)
                            {
                                endPointURL = string.Format("{0}/{1}/{2}/_apis/git/repositories/{3}/items?path={4}&api-version=5.1", devopsApiEndpoint, organizationName, projectName, repoModel.Id, path);

                                // Don't Remove this comment
                                //if (returnContentType == ReturnContentType.Image)
                                //{
                                //    endPointURL = endPointURL + "&%24format=octetStream";
                                //}

                                apiResponse = HttpHelper.SendRequestWithBasicAuthentication(basicAuth, HttpMethod.Get, endPointURL).GetAwaiter().GetResult();

                                if (apiResponse.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(apiResponse.RawResponse))
                                {
                                    privateAccessReponse.ResponseContentType = apiResponse.ResponseContentType;
                                    privateAccessReponse.RawResponse = apiResponse.RawResponse;
                                    privateAccessReponse.ByteResponse = apiResponse.ByteResponse;
                                }
                                else
                                {
                                    //privateAccessReponse.ErrorMessage = string.Format("Error : {0} : {1} ", apiResponse.StatusCode, JsonSerializer.Deserialize<HttpResponseError>(apiResponse.RawResponse, this.options).Message);
                                    privateAccessReponse.ErrorMessage = string.Format("Error : {0} : {1} ", apiResponse.StatusCode, apiResponse.RawResponse);
                                }
                            }
                            else
                            {
                                privateAccessReponse.ErrorMessage = string.Format("Unable to find repository or project {0}", projectName);
                            }
                        }
                        else
                        {
                            privateAccessReponse.ErrorMessage = string.Format("Failed to find repository or project");
                        }
                    }
                    else
                    {
                        // privateAccessReponse.ErrorMessage = string.Format("Error : {0} : {1} ", apiResponse.StatusCode, JsonSerializer.Deserialize<HttpResponseError>(apiResponse.RawResponse, this.options).Message);
                        privateAccessReponse.ErrorMessage = string.Format("Error : {0} : {1} ", apiResponse.StatusCode, apiResponse.RawResponse);
                    }
                }
                else
                {
                    Log.ErrorFormat("Not able to find PATToken for organization {0}", organizationName);
                    privateAccessReponse.ErrorMessage = string.Format("Not able to find PATToken for organization {0}", organizationName);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception while getting response from azure devops : {0}", ex.ToString());
                privateAccessReponse.ErrorMessage = ex.Message.ToString();
            }

            return privateAccessReponse;
        }

        public string GetFilePath(string url)
        {
            string[] splitUrl = url.Split('/');
            string path = string.Empty;
            bool isPathWordFound = false;
            for (int i = 2; i < splitUrl.Length; i++)
            {
                if (Convert.ToString(splitUrl[i]).Equals("path") || isPathWordFound == true)
                {
                    isPathWordFound = true;
                    if (!Convert.ToString(splitUrl[i]).Equals("path"))
                    {
                        path = string.IsNullOrEmpty(path) ? Convert.ToString(splitUrl[i]) : string.Format("{0}/{1}", path, Convert.ToString(splitUrl[i]));
                    }
                }
            }

            return path;
        }
    }
}
