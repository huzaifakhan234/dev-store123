using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevStore.Bff.Compras.Services.gRPC;
using DevStore.ShoppingCart.API.Services.gRPC;
using DevStore.WebAPI.Core.Extensions;

namespace DevStore.Bff.Compras.Configuration
{
    public static class GrpcConfig
    {
        public static void ConfigureGrpcServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<GrpcServiceInterceptor>();

            services.AddScoped<IShoppingCartGrpcService, ShoppingCartGrpcService>();

            services.AddGrpcClient<ShoppingCartOrders.ShoppingCartOrdersClient>(options =>
            {
                options.Address = new Uri(configuration["ShoppingCartUrl"]);
            })
                .AddInterceptor<GrpcServiceInterceptor>()
                .AllowSelfSignedCertificate();
        }
    }
}