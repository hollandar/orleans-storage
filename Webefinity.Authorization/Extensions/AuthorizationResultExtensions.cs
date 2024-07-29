using Microsoft.AspNetCore.Authorization;
using Webefinity.Results;

namespace Webefinity.Authorization.Extensions
{
    public static class AuthorizationResultExtensions
    {
        public static Result AsResult(this AuthorizationResult authorizationResult, string message = "User is not authorized")
        {
            if (authorizationResult.Succeeded)
            {
                return Result.Ok();
            }

            return Result.Fail(authorizationResult.Failure.FailureReasons.FirstOrDefault()?.Message ?? message, ResultReasonType.Unauthorized);
        }
    }
}
