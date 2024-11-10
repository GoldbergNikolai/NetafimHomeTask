using TimerService.Services;

namespace TimerService
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection? Configurations(this IServiceCollection? services) 
        {
            //Inits
            services?.AddControllers();
            services?.AddEndpointsApiExplorer();
            services?.AddSwaggerGen();
            services?.AddHttpClient();
            services?.AddAutoMapper(typeof(Program).Assembly);
            //DI
            services?.DependencyIjections();

            return services;
        }

        public static IServiceCollection? DependencyIjections(this IServiceCollection? services)
        {
            return services?.AddTransient<ITimerLogicService, TimerLogicService>();
        }
    }
}
