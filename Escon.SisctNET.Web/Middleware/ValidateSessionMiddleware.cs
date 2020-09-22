using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Middleware
{
    public class ValidateSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //if (SessionManager.GetLoginInSession().Equals(null))
            //{
            //    context.Response.Redirect("/");
            //}

            await _next.Invoke(context);
        }

    }
}
