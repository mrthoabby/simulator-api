using ProductManagementSystem.Application.AppEntities.UserPlans.Repository;
using System.Security.Claims;

namespace ProductManagementSystem.Application.Common.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserPlanRepository userPlanRepository)
    {
        var path = context.Request.Path.Value?.ToLower();

        if (IsAuthEndpoint(path))
        {
            await _next(context);
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthorized access to {Path} from {IP}",
                path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Authentication is required to access this resource"
            });
            return;
        }

        var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
        var userPlans = await userPlanRepository.GetAllWhereExistsAsync(userEmail!);

        var additionalClaims = new List<Claim>();

        var allUserPlansActive = userPlans.Where(p => p.IsActive).ToList();
        if (allUserPlansActive.Count == 0)
        {
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.IsPlanActive, "false"));
        }
        else
        {
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.IsPlanActive, "true"));

            string maxProducts = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxProducts).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxProducts, maxProducts));

            string maxUsers = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxUsers).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxUsers, maxUsers));

            string maxCompetitors = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxCompetitors).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxCompetitors, maxCompetitors));

            string maxCustomDeductions = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxCustomDeductions).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxCustomDeductions, maxCustomDeductions));

            string maxSimulations = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxSimulations).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxSimulations, maxSimulations));

            string maxActiveSessionDevices = allUserPlansActive.Max(p => p.Subscription.Restrictions.MaxActiveSessionDevices).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.MaxActiveSessionDevices, maxActiveSessionDevices));

            string isPDFExportSupported = allUserPlansActive.Any(p => p.Subscription.Restrictions.IsPDFExportSupported).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.IsPDFExportSupported, isPDFExportSupported));

            string isSimulationComparisonSupported = allUserPlansActive.Any(p => p.Subscription.Restrictions.IsSimulationComparisonSupported).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.IsSimulationComparisonSupported, isSimulationComparisonSupported));

            string isExcelExportSupported = allUserPlansActive.Any(p => p.Subscription.Restrictions.IsExcelExportSupported).ToString();
            additionalClaims.Add(new Claim(AuthMiddlewareValues.Claims.IsExcelExportSupported, isExcelExportSupported));
        }

        var claimsIdentity = new ClaimsIdentity(additionalClaims, "AuthMiddleware");
        context.User.AddIdentity(claimsIdentity);

        var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;

        _logger.LogInformation("Authorized access to {Path} by user {UserEmail} ({UserName}) from {IP}",
            path, userEmail, userName, context.Connection.RemoteIpAddress);

        await _next(context);
    }

    private static bool IsAuthEndpoint(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.StartsWith("/api/auth/") ||
               path.StartsWith("/swagger") ||
               path.StartsWith("/swagger-ui") ||
               path == "/" ||
               path.StartsWith("/favicon");
    }
}

public static class AuthMiddlewareValues
{
    public static class Claims
    {
        public const string IsPlanActive = "is_plan_active";
        public const string MaxProducts = "max_products";
        public const string MaxUsers = "max_users";
        public const string MaxCompetitors = "max_competitors";
        public const string MaxCustomDeductions = "max_custom_deductions";
        public const string MaxSimulations = "max_simulations";
        public const string MaxActiveSessionDevices = "max_active_session_devices";
        public const string IsPDFExportSupported = "is_pdf_export_supported";
        public const string IsSimulationComparisonSupported = "is_simulation_comparison_supported";
        public const string IsExcelExportSupported = "is_excel_export_supported";
    }

}
