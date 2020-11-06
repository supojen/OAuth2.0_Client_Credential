# Client Credentials 的好範例
<center>
沒有客戶端應用, 直接用 Post Man 附上 Access Token 的範例

[Tutorial](https://www.youtube.com/watch?v=LVYEqNkf3aI)
</center>

<br>
<br>

### IdentityServer4
___

* 編寫 Project 的部分
    
    1. 新增一個 empty web dotnet core application

    1. 安裝 Nuget Package
        > IdentityServer4 <br>
        > IdentityServer.EntityFramework
    
    1. 新增檔案 config.cs
        ```c#
        public class Config
        {

            /*
             * 說明:
             *  ApiResource 會把很多 Scope 給包裝起來, 
             *  ApiResource 代表一個 AUdience
             */
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

            /*
             * 說明:
             *  ApiScope 代表客戶端應用的使用權限
             */
            public static IEnumerable<ApiScope> GetApiScopes()
            {
                return new[]
                {
                    new ApiScope(name: "apiscope", displayName: "Access My Resource API")
                };
            }
            
            /*
             * 說明:
             *  Client 代表客戶端應用
             */
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
        ```
    1. Register IdentityServer 
        ```c#
            services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddOperationalStore( options =>
            {
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30; // Interval in seconds
            })
            .AddInMemoryApiScopes(Config.GetApiScopes())
            .AddInMemoryApiResources(Config.GetApiResources())
            .AddInMemoryClients(Config.GetClients());
        ```
    1. 加入 IdentityServer Middleware
        ```c#
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 加入 IdentityServer Middleware
        app.UseIdentityServer();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        });
        ```

* 請求 AccessToken 
    * Endpoint 是 https://localhost:44360/**connect/token**
    
    <p>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    當發出請求到 Access Token Endpoint 時, 
    要在 body 加入以 x-www-form-urlencoded 為格式的資料

    <div style="margin-left:90px;">

    |Key            |value             |
    |---------------|------------------|
    |grant_type     |client_credentials|
    |client_id      |secret_client_id  |
    |client_secret  |secret            |
    |scope          |apiscope          |

    </div>
    </P>


<br>
<br>
<br>

### 被保護 API 資源
___

1. 安裝 nuget package
    > Microsoft.AspNetCore.Authentication.JwtBearer

2. Resgister Authentication
    ```c#
    /* Resgister Authentication */
    services.AddAuthentication( options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o => {
        o.Authority = "https://localhost:44360";
        o.Audience = "myresourceapi";
        o.RequireHttpsMetadata = false;
    });

    services.AddControllers();
    ```
3. Add Authentication Middelware
    ```c#
    app.UseRouting();

    /* Add Authentication Middelware */
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    ```


<br>
<br>
<br>


### 注意事項
___
* With 4.X ApiScopes are required and ApiResources are now an optional way of grouping ApiScopes
<div align="center"> 以下代碼作範例 </div>

```c#
/* ApiScope 的部分 */
public static IEnumerable<ApiScope> GetApiScopes()
{
    return new List<ApiScope>
    {
        // invoice API specific scopes
        new ApiScope(name: "invoice.read",   displayName: "Reads your invoices."),
        new ApiScope(name: "invoice.pay",    displayName: "Pays your invoices."),

        // customer API specific scopes
        new ApiScope(name: "customer.read",    displayName: "Reads you customers information."),
        new ApiScope(name: "customer.contact", displayName: "Allows contacting one of your customers."),

        // shared scope
        new ApiScope(name: "manage", displayName: "Provides administrative access to invoice and customer data.")
    };
}

/* ApiResources 的部分 */
public static readonly IEnumerable<ApiResource> GetApiResources()
{
    return new List<ApiResource>
    {
        new ApiResource("invoice", "Invoice API")
        {
            Scopes = { "invoice.read", "invoice.pay", "manage" }
        },

        new ApiResource("customer", "Customer API")
        {
            Scopes = { "customer.read", "customer.contact", "manage" }
        }
    };
}
```