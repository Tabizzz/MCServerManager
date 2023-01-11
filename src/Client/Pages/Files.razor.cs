using Microsoft.AspNetCore.Components;
using WebServerManager.Client.Shared;
namespace WebServerManager.Client.Pages;

public partial class Files
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;
	
	[Parameter]
	public string? Path { get; set; }


	protected override void OnInitialized()
	{
		Layout.RequireSftp = true;
	}

	protected override void OnParametersSet()
	{
		if (string.IsNullOrWhiteSpace(Path)) Path = string.Empty;
		Path = "/" + Path;
	}
}