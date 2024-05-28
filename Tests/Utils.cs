using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    internal class Utils
    {
        public static HttpClient CreateClient(WebApplicationFactory<Program> factory, Action<IServiceCollection> setup)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => setup(services));
            }).CreateClient();
        }
    }
}
