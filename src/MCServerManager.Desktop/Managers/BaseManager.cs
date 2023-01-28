using System.Collections.Concurrent;
namespace MCServerManager.Desktop.Managers;

public class BaseManager<TKey, TValue> where TKey : notnull
{
	protected readonly IDictionary<TKey, TValue> _dictionary = 
		new ConcurrentDictionary<TKey, TValue>();

	public int Count => _dictionary.Count;
	
	public virtual TValue? this[TKey id] => _dictionary.ContainsKey(id) ? _dictionary[id] : default;
}