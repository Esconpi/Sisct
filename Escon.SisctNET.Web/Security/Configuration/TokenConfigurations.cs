﻿namespace Escon.SisctNET.Web.Security.Configuration
{
    public class TokenConfigurations
    {
        public string Audience { get; set; }

        public string Issuer { get; set; }

        public int Seconds { get; set; }
    }
}
