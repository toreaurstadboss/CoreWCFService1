using CoreWCF.Security;
using Microsoft.AspNetCore.Authentication;
using System.Net;

var builder = WebApplication.CreateBuilder();

builder.WebHost.ConfigureKestrel(opt => opt.AllowSynchronousIO = true);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

builder.Services.AddSingleton<IUserRepository, UserRepository>();

//if(ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12) == false)
//{
//    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
//}

builder.Services.AddAuthentication("Basic").
            AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
            ("Basic", null);

builder.Services.AddScoped<Service>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
//app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Only check for basic auth when path is for the TransportWithMessageCredential endpoint only
    if (context.Request.Path.StartsWithSegments("/Service.svc"))
    {
        // Check if currently authenticated
        var authResult = await context.AuthenticateAsync("Basic");
        if (authResult.None)
        {
            // If the client hasn't authenticated, send a challenge to the client and complete request
            await context.ChallengeAsync("Basic");
            return;
        }
    }
    // Call the next delegate/middleware in the pipeline.
    // Either the request was authenticated of it's for a path which doesn't require basic auth
    await next(context);
});

app.UseServiceModel(serviceBuilder =>
{
    var basicHttpBinding = new BasicHttpBinding();
    basicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
    basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
    serviceBuilder.AddService<Service>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });
    serviceBuilder.AddServiceEndpoint<Service, IService>(basicHttpBinding, "/Service.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpsGetEnabled = true;
});

app.Run();
