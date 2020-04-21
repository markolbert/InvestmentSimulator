using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using J4JSoftware.Logging;
using ObjectBinder;

namespace J4JSoftware.InvestmentSimulator
{
    public class SimulationContext : RootObjectBindingModel
    {
        private readonly IJ4JLogger _logger;

        public SimulationContext( IJ4JLoggerFactory loggerFactory )
            : base("Investment Simulator")
        {
            _logger = loggerFactory?.CreateLogger( typeof(SimulationContext) ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );

            Betas = new BetaDistribution( this, loggerFactory );

            ChildModels.Add( Betas );
        }

        public int Years { get; set; }
        public int Investments { get; set; }
        public int Simulations { get; set; }
        public double MeanMarketReturn { get; set; }
        public double StdDevMarketReturn { get; set; }
        public BetaDistribution Betas { get; }

        protected override void DefineBindings( IObjectBinder objBinder )
        {
            base.DefineBindings( objBinder );

            var binder = (ObjectBinder<SimulationContext>) objBinder;

            binder.AddOption(sc => sc.Years, "-y", "--years")
                .Description("years to simulate")
                .DefaultValue(10)
                .Validator(OptionInRange<int>.GreaterThanEqual(1));

            binder.AddOption(sc => sc.Investments, "-i", "--investments")
                .Description("investments to simulate")
                .DefaultValue(5)
                .Validator(OptionInRange<int>.GreaterThanEqual(1));

            binder.AddOption(sc => sc.Simulations, "-s", "--simulations")
                .Description("simulations to run")
                .DefaultValue(10)
                .Validator(OptionInRange<int>.GreaterThanEqual(1));

            binder.AddOption(sc => sc.MeanMarketReturn, "-r", "--meanReturn")
                .Description("mean annual rate of return for the total market")
                .DefaultValue(0.1)
                .Validator(OptionInRange<double>.GreaterThan(0.0));

            binder.AddOption(sc => sc.StdDevMarketReturn, "-d", "--stdDevReturn")
                .Description("standard deviation of total market annual rate of return")
                .DefaultValue(0.2)
                .Validator(OptionInRange<double>.GreaterThan(0.0));
        }
    }
}