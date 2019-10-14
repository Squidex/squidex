using Microsoft.Extensions.DependencyInjection;
using Squidex.ICIS.Validation.Validators;
using Squidex.Infrastructure.Commands;

namespace Squidex.ICIS.Validation
{
    public static class ValidationServiceExtensions
    {
        public static void AddValidationServices(this IServiceCollection services)
        {
            services.AddSingleton<ICommentaryValidator, CommentaryUniquenessValidator>();
            services.AddSingleton<ICommentaryValidator, CommentaryPeriodValidator>();
            services.AddSingleton<ICommentaryValidator, CommentaryCharacterCountValidator>();

            services.AddSingleton<ICustomCommandMiddleware, CommentaryCommandMiddleware>();
        }
    }
}
