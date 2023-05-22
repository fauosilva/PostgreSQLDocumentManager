using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using Infrastructure.Password;
using Infrastructure.Persistence.Dapper.PostgreSQL;
using Npgsql;
using System.Data.Common;

namespace PostgreSQLDocumentManager.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUserService, UserService>();
        }

        public static void AddRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
        }


        public static void AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DocumentManager");

            NpgsqlDataSourceBuilder npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            var npgSqlDataSource = npgsqlDataSourceBuilder.Build();

            services.AddSingleton<DbDataSource>(npgSqlDataSource);


            services.AddScoped<IPasswordVerificationService, PasswordVerificationService>();
            services.AddScoped<IHashPasswordService, HashPasswordService>();
        }

    }
}
