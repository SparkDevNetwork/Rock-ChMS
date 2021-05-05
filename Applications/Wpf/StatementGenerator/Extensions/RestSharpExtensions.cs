using System;
using System.Net;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Rock.Apps.StatementGenerator
{
    public static class RestSharpExtensions
    {
        /// <summary>
        /// Logs in to rock.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Invalid Login
        /// </exception>
        public static RestClient LoginToRock( this RestClient restClient, string userName, string password )
        {
            restClient.UseNewtonsoftJson();

            var rockLoginRequest = new RestRequest( "api/auth/login" );
            rockLoginRequest.Method = Method.POST;
            var loginParameters = new { Username = userName, Password = password };
            rockLoginRequest.AddJsonBody( loginParameters );

            restClient.CookieContainer = new System.Net.CookieContainer();
            var rockLoginResponse = restClient.Execute( rockLoginRequest );

            if ( rockLoginResponse.StatusCode.Equals( HttpStatusCode.Unauthorized ) )
            {
                throw new Exception( "Invalid Login", rockLoginResponse.ErrorException );
            }

            if ( rockLoginResponse.StatusCode != HttpStatusCode.NoContent && rockLoginResponse.StatusCode != HttpStatusCode.OK )
            {
                string message;
                if ( rockLoginResponse.ErrorException != null )
                {
                    message = rockLoginResponse.ErrorException.Message;
                    if ( rockLoginResponse.ErrorException.InnerException != null )
                    {
                        message += "\n" + rockLoginResponse.ErrorException.InnerException.Message;
                    }
                }
                else
                {
                    message = $"Error: { rockLoginResponse.StatusCode}";
                }

                throw new Exception( message, rockLoginResponse.ErrorException );
            }

            return restClient;
        }
    }
}
