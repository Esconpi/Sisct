using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly ContextDataBase _context;

        public AuthenticationRepository(ContextDataBase context)
        {
            _context = context;
        }

        public Login FindByLogin(string login)
        {
            var person = _context.Persons.Where(_ => _.Email.Equals(login)).FirstOrDefault();
            Model.Login _login = null;

            if (person != null)
            {
                _login = new Model.Login();
                _login.Email = person.Email;
                _login.Password = person.Password;
                _login.Id = person.Id;
            }

            return _login;
        }
    }
}