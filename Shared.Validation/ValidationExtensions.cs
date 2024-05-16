
using FluentValidation.Results;

namespace Shared.Validation;

public static class ValidationExtensions
{
    public static Result AsResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid) {
            return Result.Ok();
        } else
        {
            return Result.Fail(string.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage).ToArray()));
        }
       
    }
}