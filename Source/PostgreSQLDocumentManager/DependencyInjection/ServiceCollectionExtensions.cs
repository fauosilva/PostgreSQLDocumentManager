using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Authentication;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using Infrastructure.Authentication;
using Infrastructure.Password;
using Infrastructure.Persistence.Dapper.PostgreSQL;
using Infrastructure.Persistence.Files;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using PostgreSQLDocumentManager.Configuration;
using System.Data.Common;
using System.Text;

namespace PostgreSQLDocumentManager.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDocumentService, DocumentService>();
            serviceCollection.AddScoped<IUserService, UserService>();
            serviceCollection.AddScoped<ILoginService, LoginService>();
        }

        public static void AddRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IFileRepository, FileRepository>();
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IDocumentRepository, DocumentRepository>();
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DocumentManager");            
            var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            var npgSqlDataSource = npgsqlDataSourceBuilder.Build();
            services.AddSingleton<DbDataSource>(npgSqlDataSource);

            services.Configure<FileUploadConfiguration>(config.GetSection(FileUploadConfiguration.ConfigSection));
            services.Configure<AWSConfiguration>(config.GetSection(AWSConfiguration.ConfigSection));

            services.AddScoped<IAuthenticatedUserContext, AuthenticatedUserContext>();            
            services.AddScoped<IPasswordVerificationService, PasswordVerificationService>();
            services.AddScoped<IHashPasswordService, HashPasswordService>();
        }

        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(JwtConfiguration.ConfigSection);
            services.Configure<JwtConfiguration>(section);

            var jwtConfiguration = section.Get<JwtConfiguration>();
                        
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = jwtConfiguration!.Issuer,
                         ValidAudience = jwtConfiguration!.Audience,
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration!.Key)),                         
                     };
                 });
        }
    }
}
