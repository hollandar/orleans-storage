using System;

namespace Webefinity.Module.Guides.Abstractions;

public interface IGuideAvailable
{
    Task<bool> IsGuideAvailableAsync(CancellationToken cancellationToken);
    Task<bool> IsGuideHiddenAsync(CancellationToken cancellationToken);
    Task SetGuideHiddenAsync(bool hidden, CancellationToken cancellationToken);
}