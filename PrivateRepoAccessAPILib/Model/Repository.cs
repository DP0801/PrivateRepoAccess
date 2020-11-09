using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PrivateRepoAccessAPILib.Model
{
    public class GitRepository
    {
        [JsonPropertyName("private")]
        public bool IsPrivate { get; set; }

        public string Name { get; set; }
    }

    public class Repository
    {
        [JsonPropertyName("private")]
        public bool IsPrivate { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }

        public RepositoryProject Project;
    }

    public class RepositoryProject
    {
        /// <summary>Gets or sets the home page URL.</summary>
        /// <value>The home page URL.</value>
        public string Visibility { get; set; }
    }
}
