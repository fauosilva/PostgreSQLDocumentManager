using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Infrastructure.Persistence.Dapper.PostgreSQL
{
    [ExcludeFromCodeCoverage]
    public abstract class AbstractRepository
    {
        private readonly ILogger<AbstractRepository> logger;

        protected AbstractRepository(ILogger<AbstractRepository> logger)
        {
            this.logger = logger;
        }

        public async Task<T> ExecuteWithRetryOnTransientErrorAsync<T>(Func<Task<T>> func,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var contextDictionary = new Dictionary<string, object>()
            {
                { "CallerClass", GetType().Name},
                { "MemberName", memberName },
                { "SourceFilePath", sourceFilePath },
                { "SourceLineNumber", sourceLineNumber }
            };

            var context = new Context(nameof(ExecuteWithRetryOnTransientErrorAsync), contextDictionary);
            var retryPolicy = Policy.Handle<NpgsqlException>(ex => ex.IsTransient)
                .WaitAndRetryAsync(
                    retryCount: 2,
                    sleepDurationProvider: (i) => TimeSpan.FromMilliseconds(100) * 1,
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning("{OperationKey} - Delaying for {delay}ms, then making retry {retry}.", context.OperationKey, timespan.TotalMilliseconds, retryAttempt);
                        logger.LogDebug("Retry Details {MemberName} - {SourceFilePath} : {SourceLineNumber}", context["MemberName"], context["SourceFilePath"], context["SourceLineNumber"]);
                    });

            return await retryPolicy.ExecuteAsync((context, cancellationToken) => func(), context, cancellationToken);
        }
    }
}
