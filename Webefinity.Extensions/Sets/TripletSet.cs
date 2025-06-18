using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Extensions.Work;

public class Triplet<TKey>
{
    public TKey Key { get; set; }
    public string Value { get; init; } = string.Empty;
    public string DisplayValue { get; init; } = String.Empty;

    public Triplet(TKey key, string value, string displayValue)
    {
        Key = key;
        Value = value;
        DisplayValue = displayValue;
    }

    public override string ToString() => Value;
}

public abstract class TripletSet<TKey> where TKey: notnull, Enum
{
    private Dictionary<TKey, Triplet<TKey>> triplets = new();

    public TripletSet() : base() {
        if (triplets.Count == 0)
        {
            throw new Exception("TripletSet must be initialized with at least one triplet.");
        }
    }

    protected void Add(params IEnumerable<Triplet<TKey>> addTriplets)
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

    public Triplet<TKey> this[TKey key]
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

    public string GetValue(TKey key)
    {
        if (triplets.TryGetValue(key, out var triplet))
        {
            return triplet.Value;
        }
        
        throw new KeyNotFoundException($"No triplet found with key {key}.");
    }

    public string GetDisplayValue(TKey key)
    {
        if (triplets.TryGetValue(key, out var triplet))
        {
            return triplet.DisplayValue;
        }
        
        throw new KeyNotFoundException($"No triplet found with key {key}.");
    }
}