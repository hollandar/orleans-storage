﻿namespace Webefinity.Module.Messaging.Data;

public class Address
{
    public Guid Id { get; set; } = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AddressType Type { get; set; } = AddressType.None;
    public Guid MessageId { get; set; } = default!;
    public virtual Message Message { get; set; } = default!;
}