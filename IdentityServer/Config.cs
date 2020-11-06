using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class Config
    {

        /* 變成一種 group api scope 的工具 */
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("myresourceapi", "My Resource API")
                {
                    Scopes = new List<string>() 
                    {
                        "apiscope"
                    }
                }
            };
        }

        
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
                new ApiScope(name: "apiscope", displayName: "Access My Resource API")
            };
        }
        


        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "secret_client_id",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "apiscope" }
                }
            };
        }

    }
}
