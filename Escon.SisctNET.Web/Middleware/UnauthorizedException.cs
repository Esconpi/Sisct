﻿namespace Escon.SisctNET.Web.Middleware
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) : base(message, 401)
        {

        }
    }
}
