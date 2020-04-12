using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using Serilog.Events;

namespace J4JSoftware.InvestmentSimulator
{
    internal static class ServiceProvider
    {
        private static AutofacServiceProvider _svcProvider;

        public static AutofacServiceProvider Instance => _svcProvider ??= ConfigureContainer();

        private static AutofacServiceProvider ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<J4JLoggingModule>();

            builder.Register( c =>
                {
                    var console = new LogConsoleConfiguration() { MinimumLevel = LogEventLevel.Information };
                    var file = new LogFileConfiguration() { MinimumLevel = LogEventLevel.Verbose };

                    return new J4JLoggerConfiguration()
                    {
                        Channels = new List<LogChannelConfiguration>( new LogChannelConfiguration[] { console, file } )
                    };
                } )
                .As<IJ4JLoggerConfiguration>()
                .SingleInstance();

            builder.Register( ( c, p ) =>
            {
                var loggerConfig = c.Resolve<IJ4JLoggerConfiguration>();

                return loggerConfig.CreateLogger();
            } )
                .SingleInstance();

            builder.RegisterType<J4JLoggerFactory>()
                .As<IJ4JLoggerFactory>()
                .SingleInstance();

            builder.RegisterType<SimulationContext>()
                .AsSelf();

            builder.RegisterType<Simulator>()
                .AsSelf();

            return new AutofacServiceProvider( builder.Build() );
        }
    }
}
