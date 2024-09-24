using Orleans.Runtime;
using System.Diagnostics;

namespace Orleans.NpgsqlTenancy;

public class TenancyKey
{
    public TenancyKey(string tenantId, string grainId)
    {
        this.TenantId = tenantId;
        this.Id = grainId;
    }

    public string TenantId { get; init; } 
    public string Id { get; init; }

    public static string Build(string tenantId, string grainId)
    {
        if (grainId.Contains('>')) throw new Exception("Keys can not contain '>'.");
        if (tenantId.Contains('>')) throw new Exception("TenantId can not contain '>'.");
        return $">{tenantId}>{grainId}";
    }

    public string Build() => TenancyKey.Build(TenantId, Id);

    public static string BuildNull(string grainId)
    {
        if (grainId.Contains('>')) throw new Exception("Keys can not contain '>'.");
        return $">>{grainId}";
    }

    public static TenancyKey Parse(GrainId grainId)
    {
        if (grainId.Key.Value.Span[0] != '>')
        {
            throw new Exception("The key is not a tenant key, and not a null tenant key.");
        }

        if (grainId.Key.Value.Span[1] == '>')
        {
            throw new Exception("The key is a tenant key, but for the null tenant.");
        }

        var key = grainId.Key.ToString();
        if (key is null)
        {
            throw new NullReferenceException("The key is null.");
        }

        var parts = key.Split('>', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            return new TenancyKey(parts[0], parts[1]);
        }

        throw new Exception($"The key {grainId.Key.ToString()} is not a valid tenant key.");
    }

    public static string GetId(GrainId grainId) => Parse(grainId).Id;
    public static Guid GetGuid(GrainId grainId) => Guid.Parse(Parse(grainId).Id);

    public static string GetTenantId(GrainId grainId) => Parse(grainId).TenantId;

    public static string BuildAssociated(GrainId grainId, string associatedGrainId)
    {
        return Build(GetTenantId(grainId), associatedGrainId);
    }
    
    public static string BuildAssociated(GrainId grainId, Guid associatedGrainId)
    {
        return Build(GetTenantId(grainId), associatedGrainId.ToString());
    }
    
    public static string ValidateNull(GrainId grainId)
    {
        if (grainId.Key.Value.Span[0] != '>' && grainId.Key.Value.Span[1] != '>')
        {
            var key = grainId.Key.ToString();
            if (key is null)
            {
                throw new NullReferenceException("The key is null.");
            }

            var parts = key.Split('>', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0];
            }
        }

        throw new Exception($"The key {grainId.Key.ToString()} is not a valid tenant key.");
    }

    public static bool MatchTenants(GrainId grain1, GrainId grain2)
    {
        var key1 = Parse(grain1);
        var key2 = Parse(grain2);

        return String.Compare(key1.TenantId, key2.TenantId) == 0;
    }

    public bool Match(TenancyKey otherKey) => this.TenantId == otherKey.TenantId;
    public bool Match(GrainId other) => TenancyKey.Parse(other).Match(this);

    public static implicit operator string(TenancyKey key) => key.Build();

    public static void EnsureSameTenancy(GrainId grain1, GrainId grain2)
    {
        if (!MatchTenants(grain1, grain2))
        {
            throw new Exception("The grains are not in the same tenancy.");
        }
    }
}

public static class TenancyKeyExtensions
{
    public static bool MatchTenancy(this GrainId grainId, GrainId other) => TenancyKey.MatchTenants(grainId, other);
    public static bool MatchTenancy(this GrainId grainId, TenancyKey other) => TenancyKey.Parse(grainId).Match(other);
    public static TenancyKey ParseTenancy(this GrainId grainId) => TenancyKey.Parse(grainId);
    public static string GetTenancyId(this GrainId grainId) => TenancyKey.Parse(grainId).Id;
    public static Guid GetTenancyGuid(this GrainId grainId) => Guid.Parse(TenancyKey.Parse(grainId).Id);
}
