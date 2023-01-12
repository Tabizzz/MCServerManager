using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using WebServerManager.Client.Shared;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages;

public partial class Files : IDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	[Parameter] public string Path { get; set; } = "";
	
	public SftpFileEntry[]? FileEntries { get; set; }

	public bool BackgroundLoadingFiles { get; set; }

	readonly List<BreadcrumbItem> _pathItems = new();

	protected override async Task OnInitializedAsync()
	{
		Layout.RequireSftp = true;
		if(!CredentialService.SftpCredentials.IsValid) return;

		await UpdateFiles();
		
		NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
	}

	async Task UpdateFiles()
	{
		ValidatePath();
		
		if (FileSystem.HasEntriesForPath(Path))
		{
			FileEntries = FileSystem.GetCacheEntries(Path);
			BackgroundLoadingFiles = true;
			StateHasChanged();
		}
		await FileSystem.UpdatePath(Path);
		FileEntries = FileSystem.GetCacheEntries(Path);
		BackgroundLoadingFiles = false;
		StateHasChanged();
	}

	void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
	{
		FileEntries = null;
		StateHasChanged();
		InvokeAsync(async () =>
		{
			await UpdateFiles();
		});
	}

	void ParsePathItems()
	{
		_pathItems.Clear();
		var tpath = "/";
		var split = ("files" + Path).Trim('/').Split("/");
		foreach(var path in split.SkipLast(1))
		{
			tpath += path + "/";
			_pathItems.Add(new (path, tpath));
		}
		_pathItems.Add(new (split[^1], null, true));
	}

	protected override void OnParametersSet()
	{
		ValidatePath();
		ParsePathItems();
	}

	void ValidatePath()
	{
		if (string.IsNullOrWhiteSpace(Path)) Path = string.Empty;
		if(!Path.StartsWith("/"))
			Path = "/" + Path;
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}
}