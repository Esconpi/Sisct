using Escon.SisctNET.Model.ContextDataBase;
using Escon.SisctNET.Repository;
using Escon.SisctNET.Repository.Implementation;
using Escon.SisctNET.Service;
using Escon.SisctNET.Service.Implementation;
using Escon.SisctNET.Web.Email;
using Escon.SisctNET.Web.Middleware;
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
using System.Globalization;

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
                options.IdleTimeout = TimeSpan.FromMinutes(240);
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
            services.AddScoped<IAliquotRepository, AliquotRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<ICompanyCfopRepository, CompanyCfopRepository>();
            services.AddScoped<IDarRepository, DarRepository>();
            services.AddScoped<IDarDocumentRepository, DarDocumentRepository>();
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
            services.AddScoped<ISectionRepository, SectionRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ICreditBalanceRepository, CreditBalanceRepository>();
            services.AddScoped<ITaxRepository, TaxRepository>();
            services.AddScoped<IGrupoRepository, GrupoRepository>();
            services.AddScoped<ITypeNcmRepository, TypeNcmRepository>();
            services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IProduct2Repository, Product2Repository>();
            services.AddScoped<IEmailResponsibleRepository, EmailResponsibleRepository>();
            services.AddScoped<ITaxAnexoRepository, TaxAnexoRepository>();
            services.AddScoped<ICompraAnexoRepository, CompraAnexoRepository>();
            services.AddScoped<IDevoClienteRepository, DevoClienteRepository>();
            services.AddScoped<IDevoFornecedorRepository, DevoFornecedorRepository>();
            services.AddScoped<IVendaAnexoRepository, VendaAnexoRepository>();
            services.AddScoped<INatReceitaRepository, NatReceitaRepository>();
            services.AddScoped<ICsosnRepository, CsosnRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<ITaxProducerRepository, TaxProducerRepository>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<ICountyRepository, CountyRepository>();
            services.AddScoped<IIncentiveRepository, IncentiveRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IProductNoteInventoryEntryRepository, ProductNoteInventoryEntryRepository>();
            services.AddScoped<IProductNoteInventoryExitRepository, ProductNoteInventoryExitRepository>();
            services.AddScoped<IEstoqueRepository, EstoqueRepository>();
            services.AddScoped<ITaxationTypeNcmRepository, TaxationTypeNcmRepository>();
            services.AddScoped<IAccountPlanRepository, AccountPlanRepository>();

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
            services.AddScoped<IAliquotService, AliquotService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IAuthentication, Authentication>();
            services.AddScoped<ICompanyCfopService, CompanyCfopService>();
            services.AddScoped<IDarService, DarService>();
            services.AddScoped<IDarDocumentService, DarDocumentService>();
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
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICreditBalanceService, CreditBalanceService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IGrupoService, GrupoService>();
            services.AddScoped<ITypeNcmService, TypeNcmService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IProduct2Service, Product2Service>();
            services.AddScoped<IEmailResponsibleService, EmailResponsibleService>();
            services.AddScoped<ITaxAnexoService, TaxAnexoService>();
            services.AddScoped<ICompraAnexoService, CompraAnexoService>();
            services.AddScoped<IDevoClienteService, DevoClienteService>();
            services.AddScoped<IDevoFornecedorService, DevoFornecedorService>();
            services.AddScoped<IVendaAnexoService, VendaAnexoService>();
            services.AddScoped<INatReceitaService, NatReceitaService>();
            services.AddScoped<ICsosnService, CsosnService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<ITaxProducerService, TaxProducerService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICountyService, CountyService>();
            services.AddScoped<IIncentiveService, IncentiveService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IProductNoteInventoryEntryService, ProductNoteInventoryEntryService>();
            services.AddScoped<IProductNoteInventoryExitService, ProductNoteInventoryExitService>();
            services.AddScoped<IEstoqueService, EstoqueService>();
            services.AddScoped<ITaxationTypeNcmService, TaxationTypeNcmService>();
            services.AddScoped<IAccountPlanService, AccountPlanService>();
            services.AddScoped<IAccountPlanTypeService, AccountPlanTypeService>();

            services.AddScoped<Fortes.IEnterpriseService, Fortes.Implementation.EnterpriseService>();
            services.AddScoped<Fortes.IAccountPlanService, Fortes.Implementation.AccountPlanService>();
            services.AddScoped<Fortes.ICONService, Fortes.Implementation.CONService>();
            services.AddScoped<IntegrationDarWeb.IIntegrationWsDar, IntegrationDarWeb.Implementation.IntegrationWsDar>();

            try
            {
                var configSMTP = _configuration.GetSection("EmailConfiguration");
                var confs = configSMTP.Get<EmailConfiguration>();

                services.AddSingleton<IEmailConfiguration>(confs);
                services.AddTransient<IEmailService, EmailService>();
            }
            catch (Exception ex)
            {

            }
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

            var cultureInfo = new CultureInfo("pt-BR");
            cultureInfo.NumberFormat.CurrencySymbol = "R$";

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseSession();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseValidateSessionExtension();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Authentication}/{action=Index}/{id?}");
            });
        }
    }
}
