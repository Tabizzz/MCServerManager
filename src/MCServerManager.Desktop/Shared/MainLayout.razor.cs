using Microsoft.AspNetCore.Components.Routing;
namespace MCServerManager.Desktop.Shared;

public partial class MainLayout : IDisposable
{
	public bool RequireSftp { get; set; }

	protected override void OnAfterRender(bool firstRender)
	{
		if(firstRender)
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
