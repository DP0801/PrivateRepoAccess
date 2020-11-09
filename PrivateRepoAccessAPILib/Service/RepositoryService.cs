using System;
using System.Data;
using System.Data.SqlClient;
using PrivateRepoAccessAPILib.Constants;
using PrivateRepoAccessAPILib.Contracts;
using PrivateRepoAccessAPILib.Helper;

namespace PrivateRepoAccessAPILib.Service
{
    public class RepositoryService
    {
        public IRepositoryService RetrieveObject_Old(RepositoryType type, string url)
        {
            IRepositoryService factoryObject;
            //string[] splitUrl = url.Split('/');
            //string Type = splitUrl[0].ToString();
            switch (type)
            {
                case RepositoryType.AzureDevops:
                    string[] splitAzureUrl = url.Split('/');
                    factoryObject = new AzureDevopsService()
                    {
                        organizationName = splitAzureUrl[1].ToString(),
                        projectName = splitAzureUrl[3].ToString()
                    };
                    break;
                case RepositoryType.Git:
                    string[] splitGitUrl = url.Split('/');
                    factoryObject = new GitService()
                    {
                        url = url,
                        organizationName = splitGitUrl[1].ToString(),
                        projectName = splitGitUrl[2].ToString(),
                    };
                    break;
                default:
                    factoryObject = null;
                    break;
            }

            return factoryObject;
        }

        public IRepositoryService RetrieveObject(string url)
        {
            IRepositoryService repositoryObject = null;
            string[] splitUrl = url.Split('/');
            if (splitUrl.Length > 0)
            {
                string Type = splitUrl[0].ToString();

                if (Type.Contains("raw.githubusercontent.com"))
                {
                    string[] splitGitUrl = url.Split('/');
                    if (splitGitUrl.Length > 2)
                    {
                        string orgName = splitGitUrl[1].ToString();
                        string proName = splitGitUrl[2].ToString();
                        string PATToken = string.Empty;
                        var appSettingsManager = new AppSettingsManager();
                        string connetionString = appSettingsManager.Configuration.GetSection("Connectionstring").Value;
                        SqlConnection sqlConnection = new SqlConnection(connetionString);

                        try
                        {
                            sqlConnection.Open();
                            SqlCommand selectCommand = new SqlCommand("Sp_RetrieveAccessToken", sqlConnection);
                            selectCommand.CommandType = CommandType.StoredProcedure;
                            SqlParameter poTypeParam = selectCommand.Parameters.AddWithValue("@RepositoryType", RepositoryType.Git);
                            SqlParameter orgNameParam = selectCommand.Parameters.AddWithValue("@OrganizationName", orgName);
                            SqlParameter proNameParam = selectCommand.Parameters.AddWithValue("@ProjectName", proName);
                            PATToken = Convert.ToString(selectCommand.ExecuteScalar());
                            selectCommand.Dispose();
                            sqlConnection.Dispose();

                            if (!string.IsNullOrEmpty(PATToken))
                            {
                                repositoryObject = new GitService()
                                {
                                    url = url,
                                    organizationName = orgName,
                                    projectName = proName,
                                    PATToken = PATToken,
                                };
                            }
                            else
                            {
                                // string.Format("Unable to find PATToken for : {0}", url);
                            }
                        }
                        catch (Exception ex)
                        {
                            // string.Format("Exception while getting PATToken for organization {0}: {1}", orgName, ex.ToString());
                        }
                    }
                    else
                    {
                        // "Unable to find organization or repository";
                    }
                }
                else if (Type.Contains("dev.azure.com"))
                {
                    string[] splitAzureUrl = url.Split('/');
                    if (splitAzureUrl.Length > 3)
                    {
                        string orgName = splitAzureUrl[1].ToString();
                        string proName = splitAzureUrl[3].ToString();
                        string PATToken = string.Empty;
                        var appSettingsManager = new AppSettingsManager();
                        string connetionString = appSettingsManager.Configuration.GetSection("Connectionstring").Value;
                        SqlConnection sqlConnection = new SqlConnection(connetionString);

                        try
                        {
                            sqlConnection.Open();
                            SqlCommand selectCommand = new SqlCommand("Sp_RetrieveAccessToken", sqlConnection);
                            selectCommand.CommandType = CommandType.StoredProcedure;
                            SqlParameter poTypeParam = selectCommand.Parameters.AddWithValue("@RepositoryType", RepositoryType.AzureDevops);
                            SqlParameter orgNameParam = selectCommand.Parameters.AddWithValue("@OrganizationName", orgName);
                            SqlParameter proNameParam = selectCommand.Parameters.AddWithValue("@ProjectName", proName);
                            PATToken = Convert.ToString(selectCommand.ExecuteScalar());
                            selectCommand.Dispose();
                            sqlConnection.Dispose();

                            if (!string.IsNullOrEmpty(PATToken))
                            {
                                repositoryObject = new AzureDevopsService()
                                {
                                    organizationName = orgName,
                                    projectName = proName,
                                    PATToken = PATToken,
                                };
                            }
                            else
                            {
                                // string.Format("Unable to find PATToken for : {0}", url);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log.ErrorFormat("Exception while getting PATToken for organization {0}: {1}", organizationName, ex.ToString());
                        }

                    }
                    else
                    {
                        // "Unable to find organization or repository";
                    }
                }
                else
                {
                    // "URL must contains any of the repository either azure devops or git";
                }
            }
            else
            {
                // "Invalid URL";
            }

            return repositoryObject;
        }
    }
}
