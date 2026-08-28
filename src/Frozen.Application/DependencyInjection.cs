using Frozen.Application.Interfaces;
using Frozen.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Frozen.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
