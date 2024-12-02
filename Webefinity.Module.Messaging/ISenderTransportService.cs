
namespace Webefinity.Module.Messaging;

public interface ISenderTransportService
{
    Task<int> SendAsync(CancellationToken ct = default);
    Task PurgeAsync(CancellationToken ct = default);
}
