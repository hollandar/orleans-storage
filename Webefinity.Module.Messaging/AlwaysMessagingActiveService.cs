﻿using Webefinity.Modules.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public class AlwaysMessagingActiveService : IMessagingActive
{
    public Task<bool> IsMessagingAsync()
    {
        return Task.FromResult(true);
    }
}