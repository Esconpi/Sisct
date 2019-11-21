using Escon.SisctNET.Model;

namespace Escon.SisctNET.Web.Security
{
    public interface IAuthentication
    {
        ReponseAuthentication FindByLogin(Login user);
    }
}
