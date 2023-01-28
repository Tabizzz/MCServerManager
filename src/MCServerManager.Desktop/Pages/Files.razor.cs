using System.Net;
using MCServerManager.Desktop.Components.Dialogs;
using MCServerManager.Desktop.Models;
using MCServerManager.Desktop.Shared;
using MCServerManager.Desktop.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
namespace MCServerManager.Desktop.Pages;

public partial class Files : IDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	public List<SftpFileEntry>? FileEntries { get; set; }

	public bool BackgroundLoadingFiles { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await UpdateFiles();

		NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
	}

	async Task UpdateFiles(bool restore = true)
	{
		ValidatePath();
		FileEntries = null;
		StateHasChanged();

		if (restore && FileSystem.HasEntriesForPath(Path))
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
		InvokeAsync(async () => { await UpdateFiles(); });
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}

	string GetLinkFOrFileEntry(SftpFileEntry file)
	{
		var path = (file.IsFolder ? "/files" : "/raw") + file.Path;
		return path;
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
		DialogService.Show<FileCreateDialog>($"Create new file", parameters, options);

	}

	void CreateDirectory()
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path },
			{ "Directory", true }
		};
		DialogService.Show<FileCreateDialog>($"Create new directory", parameters);
	}

	bool _uploading;
	
	long _uploaded;

	double _percentage;

	long _toUpload;

	async Task UploadFiles(InputFileChangeEventArgs arg)
	{
		if(ServerManager.CurrentServer is null) return;
		var file = arg.File;
		_uploading = true;
		_percentage = 0;
		_toUpload = file.Size;
		StateHasChanged();
		
		
		try
		{
			await Sftp.UploadFile(file, PathUtils.GetFullPath(Path, file.Name), ServerManager.CurrentServer.Id, obj =>
			{
				InvokeAsync(() =>
				{
					_uploaded = (long)obj;
					_percentage = Convert.ToDouble(_uploaded * 100 / file.Size);
					StateHasChanged();
				});
			});
			
			if (Sftp.StatusCode == HttpStatusCode.Accepted)
			{
				await UpdateFiles();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("{0} not uploaded (Err: 6): {1}",
				file.Name, ex.Message);
		}
		_uploading = false;
		StateHasChanged();
	}

	string GetFileSizeStr(long fileSize)
	{
		double size = fileSize;
		if (size < 1024)
			return size + " B";
		size = Math.Round(fileSize / 1024d, 2);
		if (size < 1024)
			return size + " KB";
		size = Math.Round(fileSize / 1024d / 1024d, 2);
		if (size < 1024)
			return size + " MB";
		return size + " GB";
	}

	MudMessageBox Mbox { get; set; } = null!;
	
	async Task DeleteFile(SftpFileEntry file)
	{
		if(ServerManager.CurrentServer is null) return;

		var result = await Mbox.Show();
		if (result.HasValue && result.Value)
		{
			FileEntries?.Remove(file);
			StateHasChanged();
			await Sftp.DeleteFile(ServerManager.CurrentServer.Id, file.Path);
			if (Sftp.StatusCode == HttpStatusCode.Accepted)
			{
				Snackbar.Add((file.IsFolder ? "Folder" : "File") + " deleted", Severity.Success);
			}
			else
			{
				Snackbar.Add("Unable to delete " + (file.IsFolder ? "folder" : "file"), Severity.Error);
			}
			await UpdateFiles(false);
		}
	}

	bool _saveOpen;

	bool _downloading;
	
	async Task DownloadFile(SftpFileEntry file)
	{
		if(ServerManager.CurrentServer is null) return;
		_saveOpen = true;
		_toUpload = file.Size;
		_uploaded = 0;
		_percentage = 0;
		
		StateHasChanged();
		await Task.Delay(10);
		var dialog = NativeFileDialogSharp.Dialog.FileSave(System.IO.Path.GetExtension(file.Name).TrimStart('.'));
		_saveOpen = false;
		if (dialog.IsOk)
		{
			_downloading = true;
			await Sftp.SaveFile(ServerManager.CurrentServer.Id, file.Path, dialog.Path, obj =>
			{
				InvokeAsync(() =>
				{
					_uploaded = (long)obj;
					_percentage = Convert.ToDouble(_uploaded * 100 / file.Size);
					StateHasChanged();
				});
			});
		}
		_downloading = false;
		StateHasChanged();
	}

	async Task RenameFile(SftpFileEntry file)
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path },
			{ "Directory", file.IsFolder },
			{ "Src", file.Path }
		};
		var dialog = await DialogService.ShowAsync<RenameDialog>("Rename", parameters);
		if ((await dialog.Result).Data is true)
		{
			await UpdateFiles();
		}
	}
}