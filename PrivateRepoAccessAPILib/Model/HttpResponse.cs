using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PrivateRepoAccessAPILib.Model
{
    public class HttpResponse
    {
        /// <summary>Gets or sets the raw response.</summary>
        /// <value>The raw response.</value>
        public string RawResponse { get; set; }

        /// <summary>Gets or sets the byte response.</summary>
        /// <value>The byte response.</value>
        public byte[] ByteResponse { get; set; }

        /// <summary>Gets or sets the request URL.</summary>
        /// <value>The request URL.</value>
        public string RequestUrl { get; set; }

        /// <summary>Gets or sets the response content type.</summary>
        /// <value>The request response content type.</value>
        public string ResponseContentType { get; set; }

        /// <summary>Gets or sets the status code.</summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; set; }
    }
}
