using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static bool IsInternalServerError(Exception exception)
        {
            return GetStatusCode(exception) == 500;
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception is BaseException
                ? ((BaseException)exception).StatusCodeException
                : (int)HttpStatusCode.InternalServerError;
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCode(exception);
            var problemDetails = new ProblemDetailsFields
            {
                Status = context.Response.StatusCode,
                Title = "Error",
                Type = exception.GetType().Name,
                Detail = exception.Message,
                Instance = exception?.TargetSite?.Name ?? string.Empty,
                FieldErrors = exception.Data.Contains("FieldErrors") ? (List<FieldError>)exception.Data["FieldErrors"] : null
            };

            return context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails));
        }
    }
}
