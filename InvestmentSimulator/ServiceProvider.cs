using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace InvestmentSimulator
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

            builder.RegisterGeneric( typeof( J4JLogger<> ) )
                .As( typeof( IJ4JLogger<> ) )
                .SingleInstance();

            builder.RegisterType<SimulationContext>()
                .UsingConstructor( typeof(IJ4JLogger<SimulationContext>) )
                .AsSelf();

            return new AutofacServiceProvider( builder.Build() );
        }
    }
}
