using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Xml.Linq;


public static class HttpRequestExtensions
{

    public static string? GetAuthenticationHeaderFromSoapEnvelope(this HttpRequest request)
    {
        // read out body (wait for all bytes)
        using (var streamCopy = new MemoryStream())
        {
            request.Body.CopyTo(streamCopy);
            streamCopy.Position = 0; // rewind

            string body = new StreamReader(streamCopy).ReadToEnd();

            string authTicketFromHeader = null;

            if (body?.Contains(@"http://schemas.xmlsoap.org/soap/envelope/") == true)
            {
                XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
                var soapEnvelope = XDocument.Parse(body);
                var headers = soapEnvelope.Descendants(ns + "Header").ToList();

                foreach (var header in headers)
                {
                    var authorizationElement = header.Element("Authorization");
                    if (!string.IsNullOrWhiteSpace(authorizationElement?.Value))
                    {
                        authTicketFromHeader = authorizationElement.Value;
                        break;
                    }
                }
            }

            streamCopy.Position = 0; // rewind again
            //request.Body = streamCopy; // put back in place for downstream handlers

            //request.Headers.Add("Authorization", authTicketFromHeader);

            return authTicketFromHeader;
        }

    }

}

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{

    private readonly IUserRepository _userRepository;
    public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock, IUserRepository userRepository) :
       base(options, logger, encoder, clock)
    {
        _userRepository = userRepository;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new GenericIdentity("letmein");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        return await Task.FromResult(AuthenticateResult.Success(ticket));


        //string? authTicketFromSoapEnvelope = Request.GetAuthenticationHeaderFromSoapEnvelope();

        //if (authTicketFromSoapEnvelope != null && authTicketFromSoapEnvelope.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        //{
        //    var token = authTicketFromSoapEnvelope.Substring("Basic ".Length).Trim();
        //    var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        //    var credentials = credentialsAsEncodedString.Split(':');
        //    if (await _userRepository.Authenticate(credentials[0], credentials[1]))
        //    {
        //        //var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "Admin") };
        //        var identity = new GenericIdentity(credentials[0]);
        //        var claimsPrincipal = new ClaimsPrincipal(identity);
        //        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        //        return await Task.FromResult(AuthenticateResult.Success(ticket));
        //    }
    }


        //Response.StatusCode = 401;
        //Response.Headers.Add("WWW-Authenticate", "Basic realm=\"thoushaltnotpass.com\"");
        //return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    

}