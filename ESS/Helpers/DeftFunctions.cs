using System.Net;
using System.ServiceModel;

namespace ESS.Helpers
{
   

    public static class DeftFunctions
    {

        public static DeftLoginMode deftLoginMode = DeftLoginMode.BASIC;

        public static NetworkCredential getNetworkCredential(IConfiguration _configuration)
        {
            if (deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                return new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"]);
            }
            else
            {
                return new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"], _configuration["BC_API:Domain"]);
            }
            
        }

        public static string getUsername(IConfiguration _configuration)
        {
            return _configuration["BC_API:Username"];
        }

        public static string getPassword(IConfiguration _configuration)
        {
            return  _configuration["BC_API:Password"];
        }

        public static HttpClientCredentialType getHttpClientCredentialType()
        {

            if (deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                return HttpClientCredentialType.Basic;
            }
            else
            {
                return HttpClientCredentialType.Windows;
            }
   
        }
    }

    public  enum DeftLoginMode
    {
        BASIC = 1,
        WINDOWS = 2
    }
}
