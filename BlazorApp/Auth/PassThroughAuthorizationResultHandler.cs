using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace BlazorApp.Auth;

// Pages marked with [Authorize] also get checked by ASP.NET on the first HTTP request,
// before the Blazor circuit (and our SimpleAuthProvider) exists. There is no cookie or
// token at that point, so the check would always fail and crash with "no authentication
// scheme". This handler lets the request through; AuthorizeRouteView in Routes.razor then
// does the real check with the user from session storage.
public class PassThroughAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        return next(context);
    }
}
