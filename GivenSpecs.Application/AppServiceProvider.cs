using GivenSpecs.Application.Interfaces;
using GivenSpecs.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GivenSpecs.Application
{
    public static class AppServiceProvider
    {
        public static void AddGivenSpecsAppServices(this ServiceCollection collection, IGivenSpecsAppConfiguration config)
        {
            collection.AddSingleton<IGivenSpecsAppConfiguration>(config);
            collection.AddScoped<IStringHelperService, StringHelperService>();
            collection.AddScoped<IXunitGeneratorService, XunitGeneratorService>();
        }
    }
}
