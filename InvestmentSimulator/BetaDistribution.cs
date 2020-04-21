using System;
using J4JSoftware.Logging;
using MathNet.Numerics.Distributions;
using ObjectBinder;

namespace J4JSoftware.InvestmentSimulator
{
    public class BetaDistribution : ObjectBindingModel
    {
        private readonly IJ4JLogger _logger;

        private Beta _betaDist;

        public BetaDistribution( SimulationContext simContext, IJ4JLoggerFactory loggerFactory )
            : base( "beta", simContext )
        {
            _logger = loggerFactory?.CreateLogger( this.GetType() ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );
        }

        public double Alpha { get; set; }
        public double Beta { get; set; }
        public double Minimum { get; set; } = -1.0;
        public double Maximum { get; set; } = 1.0;

        public Beta Distribution => _betaDist ??= new Beta( Alpha, Beta );

        protected override void DefineBindings( IObjectBinder objBinder )
        {
            base.DefineBindings( objBinder );

            var binder = (ObjectBinder<BetaDistribution>) objBinder;

            binder.AddOption(b => b.Alpha, "-a")
                .Description("alpha parameter for beta distribution of investment betas")
                .DefaultValue(1.0)
                .Validator(OptionInRange<double>.GreaterThan(0.0));

            binder.AddOption(b => b.Beta, "-b")
                .Description("beta parameter for beta distribution of investment betas")
                .DefaultValue(2.0)
                .Validator(OptionInRange<double>.GreaterThan(0.0));

            binder.AddOption(b=> b.Maximum, "-x")
                .Description("maximum investment beta")
                .DefaultValue(2.0);

            binder.AddOption(b => b.Minimum, "-m")
                .Description("minimum investment beta")
                .DefaultValue(-2.0);
        }
    }
}