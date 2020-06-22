using Escon.SisctNET.Model.ContextDataBase;
using Escon.SisctNET.Repository;
using Escon.SisctNET.Repository.Implementation;
using Escon.SisctNET.Service;
using Escon.SisctNET.Service.Implementation;
using Escon.SisctNET.Web.Security;
using Escon.SisctNET.Web.Security.Configuration;
using Escon.SisctNET.Web.Security.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
namespace Escon.SisctNET.Web
{
    public class Startup
    {
        private readonly ILogger _logger;
        public IConfiguration _configuration { get; }
        public IHostingEnvironment _enviroment { get; }

        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<Startup> logger
            )
        {
            _configuration = configuration;
            _enviroment = environment;
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            var connectionString = _configuration["MySqlConnection:MySqlConnectionString"];
            services.AddDbContext<ContextDataBase>(options =>
                options.UseMySql(connectionString)
            );

            var signingConfiguration = new SigningConfigurations();
            services.AddSingleton(signingConfiguration);

            var tokenConfiguration = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(_configuration.GetSection("TokenConfigurations")).Configure(tokenConfiguration);

            services.AddSingleton(tokenConfiguration);
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfiguration.Key;
                paramsValidation.ValidAudience = tokenConfiguration.Audience;
                paramsValidation.ValidIssuer = tokenConfiguration.Issuer;

                paramsValidation.ValidateIssuerSigningKey = true;

                paramsValidation.ValidateLifetime = true;
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFunctionalityRepository, FunctionalityRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IAccessRepository, AccessRepository>();
            services.AddScoped<ICfopRepository, CfopRepository>();
            services.AddScoped<ICestRepository, CestRepository>();
            services.AddScoped<INcmRepository, NcmRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ITaxationTypeRepository, TaxationTypeRepository>();
            services.AddScoped<IProductNoteRepository, ProductNoteRepository>();
            services.AddScoped<ITaxationRepository, TaxationRepository>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<ICompanyCfopRepository, CompanyCfopRepository>();
            services.AddScoped<IDarRepository, DarRepository>();
            services.AddScoped<IAnnexRepository, AnnexRepository>();
            services.AddScoped<INcmConvenioRepository, NcmConvenioRepository>();
            services.AddScoped<ICstRepository, CstRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ICountingTypeRepository, CountingTypeRepository>();
            services.AddScoped<ITaxationNcmRepository, TaxationNcmRepository>();
            services.AddScoped<IProductIncentivoRepository, ProductIncentivoRepository>();
            services.AddScoped<IProduct1Repository, Product1Repository>();
            services.AddScoped<ITypeClientRepository, TypeClientRepository>();
            services.AddScoped<ISuspensionRepository, SuspensionRepository>();

            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IFunctionalityService, FunctionalityService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOccurrenceService, OccurrenceService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<ICfopService, CfopService>();
            services.AddScoped<ICestService, CestService>();
            services.AddScoped<INcmService, NcmService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<ITaxationTypeService, TaxationTypeService>();
            services.AddScoped<IProductNoteService, ProductNoteService>();
            services.AddScoped<ITaxationService, TaxationService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IAuthentication, Authentication>();
            services.AddScoped<ICompanyCfopService, CompanyCfopService>();
            services.AddScoped<IDarService, DarService>();
            services.AddScoped<IAnnexService, AnnexService>();
            services.AddScoped<INcmConvenioService, NcmConvenioService>();
            services.AddScoped<ICstService, CstService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ICfopTypeService, CfopTypeService>();
            services.AddScoped<ICountingTypeService, CountingTypeService>();
            services.AddScoped<ITaxationNcmService, TaxationNcmService>();
            services.AddScoped<IProductIncentivoService, ProductIncentivoService>();
            services.AddScoped<IProduct1Service, Product1Service>();
            services.AddScoped<ITypeClientService, TypeClientService>();
            services.AddScoped<ISuspensionService, SuspensionService>();

            services.AddScoped<Fortes.IEnterpriseService, Fortes.Implementation.EnterpriseService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Authentication}/{action=Index}/{id?}");
            });
        }
    }
}
