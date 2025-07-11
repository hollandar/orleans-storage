using System;

namespace Webefinity.Module.Authority.Entities;

public class UserAuthority
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string UserId { get; set; } = string.Empty;
    public string? Name { get; set; } = null;
    public string Value { get; set; } = string.Empty;
    public AuthorityType Type { get; set; } = AuthorityType.Role;
}
