using System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Webefinity.Module.Authority.Entities;

namespace Webefinity.Module.Authority.Services;

public class AuthorityService
{
    private readonly IAuthorityDbContext dbContext;

    public AuthorityService(IAuthorityDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Claim[]> GetClaimsAsync(string userId)
    {
        var authorities = dbContext.Authorities
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        var claimsList = authorities.Select(authority => new { authority.Name, authority.Value, authority.Type }).ToList().Select(authority =>
        {
            switch (authority.Type)
            {
                case AuthorityType.Role:
                    return new Claim(AuthorityClaimTypes.Role, authority.Value);
                case AuthorityType.Permission:
                    return new Claim(AuthorityClaimTypes.Permission, authority.Value);
                case AuthorityType.Claim:
                    Debug.Assert(authority.Name is not null, "Authority Name can not be null for Claim type");
                    return new Claim(authority.Name, authority.Value);
                default:
                    throw new InvalidOperationException($"Unknown authority type: {authority.Type}");
            }
        }).ToArray();

        return Task.FromResult(claimsList);
    }

    public bool HasUserInRole(string roleName)
    {
        return dbContext.Authorities
            .AsNoTracking()
            .Any(a => a.Type == AuthorityType.Role && a.Value == roleName);
    }

    public async Task AddRoleAuthorityAsync(string userId, string roleName)
    {
        var authority = new UserAuthority
        {
            UserId = userId,
            Name = AuthorityClaimTypes.Role,
            Value = roleName,
            Type = AuthorityType.Role
        };

        dbContext.Authorities.Add(authority);
        await dbContext.SaveChangesAsync();
    }
}
