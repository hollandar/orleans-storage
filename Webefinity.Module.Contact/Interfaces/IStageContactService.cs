using Webefinity.Results;

namespace Webefinity.Module.Contact.Interfaces
{
    public interface IStageContactService
    {
        Task<Result> StageContactAsync<TContactModel>(TContactModel model, string? type = null, int? contactLimit = null);
    }
}
