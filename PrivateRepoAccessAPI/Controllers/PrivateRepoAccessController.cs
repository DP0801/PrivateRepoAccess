using Microsoft.AspNetCore.Mvc;
using PrivateRepoAccessAPILib.Constants;
using PrivateRepoAccessAPILib.Service;
using System.Runtime.InteropServices;

namespace PrivateRepoAccessAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class PrivateRepoAccessController : ControllerBase
    {
        [HttpGet]
        [Route("{*url}")]
        public ActionResult<string> Get(string url)
        {
            string retResponse = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                string[] splitUrl = url.Split('/');
                if (splitUrl.Length > 0)
                {
                    RepositoryService repositoryService = new RepositoryService();

                    var retrievedObject = repositoryService.RetrieveObject(url);
                    if (retrievedObject != null)
                    {
                        if (retrievedObject as GitService != null)
                        {
                            #region Git
                            var returnReponse = retrievedObject.GetFileContent();

                            if (returnReponse != null && returnReponse.ResponseContentType != null && returnReponse.ResponseContentType.Equals("text/plain"))
                            {
                                if (!string.IsNullOrEmpty(returnReponse.RawResponse))
                                {
                                    retResponse = returnReponse.RawResponse;
                                }
                                else
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                            }
                            else if (returnReponse != null && returnReponse.ResponseContentType != null && returnReponse.ResponseContentType.Contains("image"))
                            {
                                if (returnReponse.ByteResponse != null)
                                {
                                    return File(returnReponse.ByteResponse, returnReponse.ResponseContentType);
                                }
                                else
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(returnReponse.ErrorMessage))
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                                else
                                {
                                    retResponse = "Failed to retrieve content or file.";
                                }
                            }
                            #endregion
                        }

                        if (retrievedObject as AzureDevopsService != null)
                        {
                            #region Azure Devops
                            AzureDevopsService azureDevops = retrievedObject as AzureDevopsService;
                            azureDevops.path = azureDevops.GetFilePath(url);
                            var returnReponse = azureDevops.GetFileContent();
                            if (returnReponse != null && returnReponse.ResponseContentType != null && returnReponse.ResponseContentType.Equals("application/octet-stream"))
                            {
                                if (!string.IsNullOrEmpty(returnReponse.RawResponse))
                                {
                                    retResponse = returnReponse.RawResponse;
                                }
                                else
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                            }
                            else if (returnReponse != null && returnReponse.ResponseContentType != null && returnReponse.ResponseContentType.Contains("image"))
                            {
                                if (returnReponse.ByteResponse != null)
                                {
                                    return File(returnReponse.ByteResponse, returnReponse.ResponseContentType);
                                }
                                else
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(returnReponse.ErrorMessage))
                                {
                                    retResponse = returnReponse.ErrorMessage;
                                }
                                else
                                {
                                    retResponse = "Failed to retrieve content or file.";
                                }
                            }

                            #endregion
                        }
                    }
                    else
                    {
                        retResponse = "Unable to retrieve object";
                    }
                }
                else
                {
                    retResponse = "Invalid url";
                }
            }
            else
            {
                retResponse = "Unable to find any url";
            }

            return retResponse;
        }
    }
}