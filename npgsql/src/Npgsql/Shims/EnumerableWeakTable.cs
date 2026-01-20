
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnterpriseDB.EDBClient;

#if NETFRAMEWORK || NETSTANDARD

internal class EnumerableWeakTable<TKey, TValue> where TKey : class where TValue : class
{
    private readonly ConditionalWeakTable<TKey, TValue> _table = new ConditionalWeakTable<TKey, TValue>();
    private readonly List<WeakReference> _keys = new List<WeakReference>();
    private readonly object _lock = new object();

    public void Add(TKey key, TValue value)
    {
        lock (_lock)
        {
            // Add to the actual table
            _table.Add(key, value);

            // Add a weak reference to the key so we can track it
            _keys.Add(new WeakReference(key));
        }
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> ToEnumerable()
    {
        lock (_lock)
        {
            // We iterate backward so we can safely remove dead keys
            for (var i = _keys.Count - 1; i >= 0; i--)
            {
                var weakRef = _keys[i];
                var key = weakRef.Target as TKey;

                // 1. If key is dead (collected), remove from our tracking list
                if (key == null)
                {
                    _keys.RemoveAt(i);
                    continue;
                }

                // 2. If key is alive, try to get value from the table
                if (_table.TryGetValue(key, out var value))
                {
                    yield return new KeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    // If key is alive but not in table (rare/impossible if logic is correct), remove it
                    _keys.RemoveAt(i);
                }
            }
        }
    }
}
#endif
