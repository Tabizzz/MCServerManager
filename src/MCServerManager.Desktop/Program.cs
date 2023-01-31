using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Services;
using mcswlib;
using mcswlib.ServerStatus;
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
		appBuilder.Services.AddMessagePipe();
		appBuilder.Services.AddSingleton<FileSystemService>();
		appBuilder.Services.AddSingleton<StorageService>();
		appBuilder.Services.AddSingleton<TaskService>();
		appBuilder.Services.AddSingleton<ServerPropertiesService>();
		
		appBuilder.Services.AddSingleton<ServerManager>();
		appBuilder.Services.AddSingleton<SftpConnectionsManager>();
		appBuilder.Services.AddSingleton<SftpController>();

		appBuilder.Services.AddSingleton(sp =>
		{
			var dev = new ServerStatusFactory();
			dev.AlwaysInvokeAsyncEvent = true;
			Logger.LogLevel = Types.LogLevel.None;
			var notifier = sp.GetRequiredService<IAsyncPublisher<ServerStatus>>();
			
			dev.ServerChanged += (sender, _) =>
			{
				if (sender is ServerStatus status)
				{
					notifier.PublishAsync(status, CancellationToken.None);
				}
			};
			
			return dev;
		});
		
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
			.SetGrantBrowserPermissions(true)
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