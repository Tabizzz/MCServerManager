using Blazored.LocalStorage;
using MessagePipe;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using WebServerManager.Client;
using WebServerManager.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices(c =>
{
	c.SnackbarConfiguration.PreventDuplicates = true;
	c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
	c.SnackbarConfiguration.HideTransitionDuration = 200;
	c.SnackbarConfiguration.ShowTransitionDuration = 200;
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CredentialService>();
builder.Services.AddScoped<FileSystemService>();
builder.Services.AddMessagePipe();

var host = builder.Build();
await host.Services.GetRequiredService<CredentialService>().Load();
await host.RunAsync();
