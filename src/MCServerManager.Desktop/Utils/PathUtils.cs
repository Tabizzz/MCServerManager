namespace MCServerManager.Desktop.Utils;

public static class PathUtils
{

	public static string GetFullPath(string path, string fileName)
	{
		if (OperatingSystem.IsWindows())
		{
			var pre = Path.GetFullPath(Path.Combine(path, fileName));
			if (Path.IsPathRooted(pre))
			{
				var root = Path.GetPathRoot(pre);
				pre = ("\\" + pre.TrimStart(root!.ToCharArray())).Replace('\\', '/');
			}
			return pre.Replace(Path.PathSeparator, '/');
		}

		return Path.GetFullPath(fileName, path);
	}
}