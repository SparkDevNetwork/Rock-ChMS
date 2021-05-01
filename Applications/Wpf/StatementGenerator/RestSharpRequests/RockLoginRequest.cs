using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;

namespace Rock.Apps.StatementGenerator.RestSharpRequests
{
    public class RockLoginRequest : RestRequest
    {
        public RockLoginRequest( string userName, string password ) : base( "api/auth/login" )
        {
            this.Method = Method.POST;
            var loginParameters = new { Username = userName, Password = password };
            this.AddJsonBody( loginParameters );
        }
    }
}
