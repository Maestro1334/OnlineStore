using DAL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Services;
using Service.Interfaces;
using System.Threading.Tasks;
using Webshop.Middleware;

namespace OnlineStore
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder Builder) => {
                    Builder.UseNewtonsoftJson().UseMiddleware<JwtMiddleware>();
                })
                .ConfigureOpenApi()
                .ConfigureServices(Configure)
                .Build();

            await host.RunAsync();
        }

        static void Configure(HostBuilderContext Builder, IServiceCollection Services)
        {
            Services.AddDbContext<OnlineStoreDBContext>();
            Services.AddScoped<IUserService, UserService>();
            Services.AddScoped<IAuthService, AuthService>();
            Services.AddScoped<IProductService, ProductService>();
            Services.AddScoped<IOrderService, OrderService>();
            Services.AddScoped<IReviewService, ReviewService>();
        }
    }
}