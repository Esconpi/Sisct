using System;

namespace Escon.SisctNET.Model
{
    public class ReponseAuthentication
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string AccessToken { get; set; }

        public bool Authenticated { get; set; }

        public DateTime Created { get; set; }

        public DateTime Expiration { get; set; }

        public string Message { get; set; }
    }
}
