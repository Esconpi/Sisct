using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Middleware
{
    public class ProblemDetailsFields : ProblemDetails
    {
        public List<FieldError> FieldErrors { get; set; }
    }
}
