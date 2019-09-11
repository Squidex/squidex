using Microsoft.Extensions.DependencyInjection;
using Squidex.ICIS.Validation.Validators;
using Squidex.Infrastructure.Commands;

namespace Squidex.ICIS.Validation
{
    public static class ValidationServiceExtensions
    {
        public static void AddValidationServices(IServiceCollection services)
        {
            AddValidationCommandMiddleware(services);
        }

        private static void AddValidationCommandMiddleware(IServiceCollection services)
        {
            services.AddSingleton<ICommentaryValidator, CommentaryUniquenessValidator>();
            services.AddSingleton<ICommentaryValidator, CommentaryCharacterCountValidator>();

            services.AddSingleton<ICustomCommandMiddleware, CommentaryCommandMiddleware>();
        }
    }
}
