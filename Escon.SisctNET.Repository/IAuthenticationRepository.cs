using Escon.SisctNET.Model;

namespace Escon.SisctNET.Repository
{
    public interface IAuthenticationRepository
    {
        Login FindByLogin(string login);
    }
}