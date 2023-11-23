using System.IO.Pipelines;
using System.Text;
using System.Xml.Linq;

public static class HttpRequestExtensions
{

    public static async Task<string?> GetAuthenticationHeaderFromSoapEnvelope(this HttpRequest request)
    {
        ReadResult requestBodyInBytes = await request.BodyReader.ReadAsync();
        string body = Encoding.UTF8.GetString(requestBodyInBytes.Buffer.FirstSpan);
        request.BodyReader.AdvanceTo(requestBodyInBytes.Buffer.Start, requestBodyInBytes.Buffer.End);

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

        return authTicketFromHeader;
    }

}
