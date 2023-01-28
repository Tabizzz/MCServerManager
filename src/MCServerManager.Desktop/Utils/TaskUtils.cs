namespace MCServerManager.Desktop.Utils;

public static class TaskUtils
{
	public static Task<T2> Run<T1, T2>(Func<T1, T2> func, T1 arg) => 
		Task.Run(()=>func(arg));
	
	public static Task Run<T1>(Action<T1> func, T1 arg) => 
		Task.Run(()=>func(arg));
}