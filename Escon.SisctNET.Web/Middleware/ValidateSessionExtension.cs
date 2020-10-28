using Microsoft.AspNetCore.Builder;

namespace Escon.SisctNET.Web.Middleware
{
    public static class ValidateSessionExtension
    {
        public static IApplicationBuilder UseValidateSessionExtension(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidateSessionMiddleware>();
        }
    }
}