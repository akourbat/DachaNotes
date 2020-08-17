using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorClientApp.ViewModels;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat;
using ReactiveUI;
using BlazorCientApp.Pages;
using Refit;
using BlazorCientApp.Services;
using BlazorCientApp.ViewModels;

namespace BlazorCientApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            //builder.Services.AddHttpClient("BookService", client => client.BaseAddress = new Uri("http://Localhost:7071/"));
            builder.Services.AddTransient<IViewFor<BookVM>, BookView>();
            builder.Services.AddTransient<IViewFor<BookMainViewModel>, BookMainView>();
            builder.Services.AddScoped<BookMainViewModel>();
            builder.Services.AddTransient<BookVM>();
            builder.Services.AddRefitClient<IRefitBookService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("http://Localhost:7071"));
            builder.RootComponents.Add<App>("app");

            var host = builder.Build();
            host.Services.UseMicrosoftDependencyResolver();

            await host.RunAsync();
        }
    }
}
