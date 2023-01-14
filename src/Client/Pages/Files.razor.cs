using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using WebServerManager.Client.Dialogs;
using WebServerManager.Client.Shared;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages;

public partial class Files : IDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;
	
	public SftpFileEntry[]? FileEntries { get; set; }

	public bool BackgroundLoadingFiles { get; set; }
	
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
		var res = await FileSystem.UpdatePath(Path);
		if (res.Item1 == Path)
		{
			FileEntries = res.Item2;
		}
		BackgroundLoadingFiles = false;
		StateHasChanged();
	}

	void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
	{
		if (!NavigationManager.ToBaseRelativePath(e.Location).StartsWith("files"))
			return;
		FileEntries = null;
		StateHasChanged();
		InvokeAsync(async () => { await UpdateFiles(); });
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}

	string GetLinkFOrFileEntry(SftpFileEntry file)
	{
		var path = (file.IsFolder ? "/files" : "raw") + file.Path;
		if (file.IsFolder || System.IO.Path.GetExtension(file.Path) is ".yml" or ".json" or ".txt" or ".properties")
		{
			return path;
		}
		return null!;
	}

	void CreateFile()
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path }
		};
		var options = new DialogOptions
		{
			CloseButton = true,
			MaxWidth = MaxWidth.Medium,
			FullWidth = true,
			Position = DialogPosition.TopCenter
		};
		DialogService.Show<FileCreateDialog>($"Create new file",parameters, options);
		
	}

	void CreateDirectory()
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path },
			{ "Directory", true}
		};
		var options = new DialogOptions
		{
			CloseButton = true,
			MaxWidth = MaxWidth.Medium,
			FullWidth = true,
			Position = DialogPosition.TopCenter
		};
		DialogService.Show<FileCreateDialog>($"Create new directory",parameters, options);
	}
}