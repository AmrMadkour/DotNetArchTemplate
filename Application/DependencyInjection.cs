using Application.Services;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<ILogTestService, LogTestService>();

        // Parked Strategy/Factory scaffold (see Domain/Services/IDiscountStrategy) —
        // registered so it's resolvable, but nothing calls it yet. See the comment
        // in QuoteService.PrepareQuote for how it would be used.
        //services.AddScoped<IDiscountStrategy, PercentageDiscountStrategy>();
        //services.AddScoped<IDiscountStrategy, FlatAmountDiscountStrategy>();
        //services.AddScoped<DiscountStrategyFactory>();

        return services;
    }
}
