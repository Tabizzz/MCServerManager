using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace WebServerManager.Client.Components;

public class PathComponentBase : ComponentBase
{
	[Parameter] public string Path { get; set; } = "";
	
	protected List<BreadcrumbItem> PathItems { get; } = new();
	
	protected void ParsePathItems()
	{
		PathItems.Clear();
		var tpath = "/";
		var split = ("files" + Path).Trim('/').Split("/");
		foreach(var path in split.SkipLast(1))
		{
			tpath += path + "/";
			PathItems.Add(new (path, tpath));
		}
		PathItems.Add(new (split[^1], null, true));
	}

	protected override void OnParametersSet()
	{
		ValidatePath();
		ParsePathItems();
	}

	protected void ValidatePath()
	{
		if (string.IsNullOrWhiteSpace(Path)) Path = string.Empty;
		if(!Path.StartsWith("/"))
			Path = "/" + Path;
	}
}