using System;
using System.Collections.Generic;
using System.Text;

namespace PrivateRepoAccessAPILib.Model
{
    public class PrivateAccessReponse
    {
        public string RawResponse { get; set; }

        public byte[] ByteResponse { get; set; }

        public string ResponseContentType { get; set; }

        public string ErrorMessage { get; set; }
    }
}
