using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using Escon.SisctNET.Web.Security.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace Escon.SisctNET.Web.Security.Implementation
{
    public class Authentication : IAuthentication
    {
        private readonly IAuthenticationRepository _repository;
        private SigningConfigurations _signing;
        private TokenConfigurations _tokenConfigurations;

        public Authentication(IAuthenticationRepository repository,
            SigningConfigurations signing,
            TokenConfigurations tokenConfigurations)
        {
            _repository = repository;
            _signing = signing;
            _tokenConfigurations = tokenConfigurations;
        }

        public ReponseAuthentication FindByLogin(Login user)
        {
            bool credentialIsValid = false;
            Model.Login baseUser = null;

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                baseUser = _repository.FindByLogin(user.Email);
                credentialIsValid = (baseUser != null && user.Email == baseUser.Email && user.AccessKey == baseUser.AccessKey);
            }

            if (credentialIsValid)
            {
                ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(user.Email, "Email"),
                        new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString("N")),
                            new Claim(JwtRegisteredClaimNames.UniqueName,user.Email),
                        }
                    );

                DateTime createDate = DateTime.Now;
                DateTime expirationDate = createDate + TimeSpan.FromSeconds(_tokenConfigurations.Seconds);

                var handler = new JwtSecurityTokenHandler();
                var token = CreateToken(identity, createDate, expirationDate, handler);

                return SuccessObject(createDate, expirationDate, token, Convert.ToInt64(baseUser.Id), baseUser.Email);
            }
            else
            {
                return ExceptionObject();
            }
        }

        private string CreateToken(ClaimsIdentity identity, DateTime createDate, DateTime expirationDate, JwtSecurityTokenHandler handler)
        {
            var securityToken = handler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Issuer = _tokenConfigurations.Issuer,
                Audience = _tokenConfigurations.Audience,
                SigningCredentials = _signing.SigningCredentials,
                Subject = identity,
                NotBefore = createDate,
                Expires = expirationDate
            });

            var token = handler.WriteToken(securityToken);
            return token;
        }

        private Model.ReponseAuthentication ExceptionObject()
        {
            return new ReponseAuthentication()
            {
                Authenticated = false,
                Message = "Failed to authenticated"
            };
        }

        private Model.ReponseAuthentication SuccessObject(DateTime createDate, DateTime expirationDate, string token, long id, string email)
        {
            return new ReponseAuthentication()
            {
                Authenticated = true,
                Created = createDate,
                Expiration = expirationDate,
                AccessToken = token,
                Message = "ok",
                Id = id,
                Email = email
            };
        }
    }
}
