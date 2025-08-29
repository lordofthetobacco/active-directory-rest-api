using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireScopeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _requiredScopes;
        private readonly bool _requireAllScopes;

        public RequireScopeAttribute(string scope)
        {
            _requiredScopes = new[] { scope };
            _requireAllScopes = true;
        }

        public RequireScopeAttribute(params string[] scopes)
        {
            _requiredScopes = scopes;
            _requireAllScopes = true;
        }

        public RequireScopeAttribute(bool requireAllScopes, params string[] scopes)
        {
            _requiredScopes = scopes;
            _requireAllScopes = requireAllScopes;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var apiTokenService = httpContext.RequestServices.GetRequiredService<IApiTokenService>();

            // Extract token from context items (set by middleware)
            if (!httpContext.Items.TryGetValue("ApiToken", out var apiTokenObj) || apiTokenObj == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the actual token from the request headers
            var token = ExtractTokenFromHeaders(httpContext.Request.Headers);
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check scopes
            bool hasRequiredScopes;
            if (_requireAllScopes)
            {
                hasRequiredScopes = await apiTokenService.HasAllScopesAsync(token, _requiredScopes);
            }
            else
            {
                hasRequiredScopes = await apiTokenService.HasAnyScopeAsync(token, _requiredScopes);
            }

            if (!hasRequiredScopes)
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        private string? ExtractTokenFromHeaders(Microsoft.AspNetCore.Http.IHeaderDictionary headers)
        {
            // Check Authorization header
            if (headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authValue) && authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return authValue.Substring("Bearer ".Length).Trim();
                }
            }

            // Check X-API-Key header as alternative
            if (headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                return apiKeyHeader.FirstOrDefault();
            }

            return null;
        }
    }
}
