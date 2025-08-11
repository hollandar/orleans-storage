using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Extensions.Work;

public class Triplet<TKey, TData>
{
    public TKey Key { get; set; } = default!;
    public string Value { get; init; } = string.Empty;
    public TData Data { get; init; } = default!;

    public Triplet(TKey key, string value, TData data)
    {
        Key = key;
        Value = value;
        Data = data;
    }

    public override string ToString() => Value;
    public override int GetHashCode() => Key?.GetHashCode() ?? 0;
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Triplet<TKey, TData> triplet => this.Key?.Equals(triplet.Key) ?? false,
            TKey key => Key?.Equals(key) ?? false,
            _ => false
        };
    }
}

public abstract class TripletSet<TKey, TData> where TKey: notnull, Enum
{
    private Dictionary<TKey, Triplet<TKey, TData>> triplets = new();
    public IEnumerable<Triplet<TKey, TData>> Values => this.triplets.Values;

    protected TripletSet() : base() {
    }

    protected void Add(params IEnumerable<Triplet<TKey, TData>> addTriplets)
    {
        foreach (var triplet in addTriplets)
        {
            if (triplet == null) throw new ArgumentNullException(nameof(triplet));
            if (triplets.ContainsKey(triplet.Key)) throw new ArgumentException($"Triplet with key {triplet.Key} already exists.", nameof(triplet));

            triplets[triplet.Key] = triplet;
        }
    }

    public bool ContainsKey(TKey key)
    {
        return triplets.ContainsKey(key);
    }   

    public bool ContainsValue(string value)
    {
        return triplets.Values.Any(t => t.Value.Equals(value));
    }

    public Triplet<TKey, TData> this[string key]
    {
        get
        {
            var triplet = this.triplets.Values.Where(r => r.Value == key).FirstOrDefault();
            if (triplet is not null)
            {
                return triplet;
            }
            throw new KeyNotFoundException($"Triplet with key '{key}' not found.");
        }
    }

    public Triplet<TKey, TData> OrFirst(string? key)
    {
        if (key is null) return triplets.Values.First();
        return this[key];
    }

    public Triplet<TKey, TData> OrFirst(TKey? key)
    {
        if (key is null) return triplets.Values.First();
        return this[key];
    }

    public Triplet<TKey, TData> this[TKey key]
    {
        get
        {
            if (triplets.TryGetValue(key, out var triplet))
            {
                return triplet;
            }
            throw new KeyNotFoundException($"Triplet with key {key} not found.");
        }
    }

    public TKey GetKey(string value, TKey? def = default(TKey))
    {
        if (this.ContainsValue(value))
        {
            return this[value].Key;
        }

        return def ?? throw new KeyNotFoundException($"No triplet found with value '{value}'.");
    }

    public string GetValue(TKey key)
    {
        if (triplets.TryGetValue(key, out var triplet))
        {
            return triplet.Value;
        }
        
        throw new KeyNotFoundException($"No triplet found with key {key}.");
    }

    public TData GetData(TKey key)
    {
        if (triplets.TryGetValue(key, out var triplet))
        {
            return triplet.Data;
        }
        
        throw new KeyNotFoundException($"No triplet found with key {key}.");
    }

    public IEnumerable<KeyValuePair<string, TData>> GetKeyValues() =>
        this.triplets.Values.Select(t => new KeyValuePair<string, TData>(t.Value, t.Data));

    public IEnumerable<Triplet<TKey, TData>> FromString(string? s)
    {
        var items = (s ?? String.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in items)
        {
            if (item is not null && ContainsValue(item))
            {
                yield return this[item];
            }
        }
    }

    public override string ToString()
    {
        return ToString(triplets.Values);
    }

    public string ToString(params IEnumerable<Triplet<TKey, TData>> triplets)
    {
        return string.Join(',', triplets.Select(r => r.Value));
    }
}