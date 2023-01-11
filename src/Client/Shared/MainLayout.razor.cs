using Microsoft.AspNetCore.Components.Routing;
namespace WebServerManager.Client.Shared;

public partial class MainLayout : IDisposable
{
	public bool RequireSftp { get; set; }

	protected override void OnInitialized()
	{
		NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
	}

	void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
	{
		RequireSftp = false;
		StateHasChanged();
	}

	void GotoAuth()
	{
		NavigationManager.NavigateTo("/");
	}
	
	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}
}
