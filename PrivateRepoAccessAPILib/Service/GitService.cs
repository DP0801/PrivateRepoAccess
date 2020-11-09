using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using log4net;
using PrivateRepoAccessAPILib.Constants;
using PrivateRepoAccessAPILib.Contracts;
using PrivateRepoAccessAPILib.Helper;
using PrivateRepoAccessAPILib.Model;

namespace PrivateRepoAccessAPILib.Service
{
    public class GitService: IRepositoryService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GitService));

        public string url;
        public string projectName;
        public string organizationName;
        public string PATToken;
         
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
                    string baseurl = string.Format("https://{0}", url);
                    var apiResponse = HttpHelper.SendRequest(PATToken, HttpMethod.Get, baseurl, null, null, "application/json", null, "git").GetAwaiter().GetResult();
                    if (apiResponse.StatusCode == HttpStatusCode.OK)
                    {
                        privateAccessReponse.RawResponse = apiResponse.RawResponse;
                        privateAccessReponse.ResponseContentType = apiResponse.ResponseContentType;
                        privateAccessReponse.ByteResponse = apiResponse.ByteResponse;
                    }
                    else
                    {
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
                Log.ErrorFormat("Exception while getting response from git : {0}", ex.ToString());
                privateAccessReponse.ErrorMessage = ex.Message.ToString();
            }

            return privateAccessReponse;
        }
    }
}
