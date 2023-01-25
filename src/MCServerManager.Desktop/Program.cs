using Blazored.LocalStorage;
using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Services;
using MessagePipe;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Photino.Blazor;
namespace MCServerManager.Desktop;

class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

		appBuilder.Services
			.AddLogging();
	
		// register root component and selector
		appBuilder.RootComponents.Add<App>("app");
		appBuilder.RootComponents.Add<HeadOutlet>("head::after");
		appBuilder.Services.AddMudServices(c =>
		{
			c.SnackbarConfiguration.PreventDuplicates = true;
			c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
			c.SnackbarConfiguration.HideTransitionDuration = 200;
			c.SnackbarConfiguration.ShowTransitionDuration = 200;
		});
		appBuilder.Services.AddBlazoredLocalStorage();
		appBuilder.Services.AddMessagePipe();
		appBuilder.Services.AddScoped<CredentialService>();
		appBuilder.Services.AddScoped<FileSystemService>();
		appBuilder.Services.AddScoped<ServerManager>();
		appBuilder.Services.AddScoped<ServerPropertiesService>();
		
		appBuilder.Services.AddSingleton<CredentialManager>();
		appBuilder.Services.AddSingleton<SftpConnectionsManager>();
		appBuilder.Services.AddSingleton<AuthController>();
		appBuilder.Services.AddSingleton<SftpController>();
		
		var app = appBuilder.Build();
		

#if RELEASE
		app.MainWindow.DevToolsEnabled = false;
		app.MainWindow.ContextMenuEnabled = false;
#endif
		app.MainWindow.LogVerbosity = 0;
		
		// customize window
		app.MainWindow
			.SetIconFile("favicon.ico")
#if DEBUG
			.SetTitle("MC Server Manager (Debug)")
#else
			.SetTitle("MC Server Manager")
#endif
			.Center()
			.SetUseOsDefaultSize(true);

		app.MainWindow.WindowCreated += (_, _) =>
		{
			app.MainWindow.SetMaximized(true);
		};

		AppDomain.CurrentDomain.UnhandledException += (_, error) =>
		{
			app.MainWindow.OpenAlertWindow("Fatal exception", error.ExceptionObject.ToString());
		};
		app.Run();

	}
}