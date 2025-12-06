using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorGame.Client.Services
{
    public class ApiLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"API Request: {request.Method} {request.RequestUri}");
            if (request.Headers.Authorization != null)
            {
                Console.WriteLine("Authorization header present");
            }
            else
            {
                Console.WriteLine("Authorization header NOT present");
            }

            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine($"API Response: {(int)response.StatusCode} {response.ReasonPhrase}");
            return response;
        }
    }
}
