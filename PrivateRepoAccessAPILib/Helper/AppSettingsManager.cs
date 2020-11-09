using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PrivateRepoAccessAPILib.Helper
{
    public class AppSettingsManager
    {
        public AppSettingsManager()
        {
            this.Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).AddEnvironmentVariables().Build();
        }

        public IConfiguration Configuration { get; set; }
    }
}
