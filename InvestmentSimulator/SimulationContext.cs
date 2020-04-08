using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using J4JSoftware.Logging;

namespace InvestmentSimulator
{
    public class SimulationContext
    {
        private class RawArgs
        {
            public int Y { get; set; }
            public int I { get; set; }
            public int S { get; set; }
            public double R { get; set; }
            public double D { get; set; }
        }

        private readonly IJ4JLogger<SimulationContext> _logger;

        private int _years = 1;
        private int _investments = 1;
        private int _simulations = 1;
        private double _maxReturn = 0.2;
        private double _maxStdDevReturn = 0.2;

        // should only be used by System.CommandLine
        public SimulationContext()
        {
        }

        public SimulationContext( IJ4JLogger<SimulationContext> logger )
        {
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );
        }

        public int Years
        {
            get => _years;

            set
            {
                if( value <= 0 )
                    _logger.Error( $"{nameof(Years)} must be >= 1" );

                _years = value;
            }
        }

        public int Investments
        {
            get => _investments;

            set
            {
                if( value <= 0 )
                    _logger.Error( $"{nameof( Investments )} must be >= 1" );

                _investments = value;
            }
        }

        public int Simulations
        {
            get => _simulations;

            set
            {
                if( value <= 0 )
                    _logger.Error( $"{nameof( Simulations )} must be >= 1" );

                _simulations = value;
            }
        }

        public double MaxAnnualInvestmentReturn
        {
            get => _maxReturn;

            set
            {
                if( value <= 0 )
                    _logger.Error( $"{nameof( MaxAnnualInvestmentReturn )} must be > 0" );

                _maxReturn = value;
            }
        }

        public double MaxStdDevAnnualInvestmentReturn
        {
            get => _maxStdDevReturn;

            set
            {
                if( value <= 0 )
                    _logger.Error( $"{nameof( MaxStdDevAnnualInvestmentReturn )} must be > 0" );

                _maxStdDevReturn = value;
            }
        }

        public bool Initialize( string[] args )
        {
            var rootCommand = new RootCommand( "Investment Simulator" );

            var years = new Option<int>( new[] { "-y", "--years" }, () => 10, "years to simulate" );
            years.AddValidator( ( r ) =>
                r.GetValueOrDefault<int>() >= 1
                    ? null
                    : $"'years' must be >= 1 (default is {r.Option.GetDefaultValue()})" );
            rootCommand.AddOption( years );

            var investments = new Option<int>( new[] { "-i", "--investments" }, () => 5, "investments to simulate" );
            investments.AddValidator( ( r ) =>
                r.GetValueOrDefault<int>() >= 1
                    ? null
                    : $"'investments' must be >= 1 (default is {r.Option.GetDefaultValue()})" );
            rootCommand.AddOption( investments );

            var simulations = new Option<int>( new[] { "-s", "--simulations" }, () => 10, "simulations to run" );
            simulations.AddValidator( ( r ) =>
                r.GetValueOrDefault<int>() >= 1
                    ? null
                    : $"'simulations' must be >= 1 (default is {r.Option.GetDefaultValue()})" );
            rootCommand.AddOption( simulations );

            var maxReturn = new Option<double>( new[] { "-r", "--maxReturn" }, () => 0.20,
                "max annual rate of return for an investment" );
            maxReturn.AddValidator( ( r ) =>
                r.GetValueOrDefault<double>() > 0.0
                    ? null
                    : $"'maxReturn' must be > 0 (default is {r.Option.GetDefaultValue()})" );
            rootCommand.AddOption( maxReturn );

            var maxStdDev = new Option<double>( new[] { "-d", "--maxStdDev" }, () => 0.20,
                "max standard deviation in the annual rate of return" );
            maxStdDev.AddValidator( ( r ) =>
                r.GetValueOrDefault<double>() > 0.0
                    ? null
                    : $"'maxStdDev' must be > 0 (default is {r.Option.GetDefaultValue()})" );
            rootCommand.AddOption( maxStdDev );

            //var binder = new ModelBinder<SimulationContext>();

            //binder.BindMemberFromValue(sc=>sc.Investments, investments);
            //binder.BindMemberFromValue(sc=>sc.MaxAnnualInvestmentReturn, maxReturn);
            //binder.BindMemberFromValue(sc=>sc.MaxStdDevAnnualInvestmentReturn, maxStdDev);
            //binder.BindMemberFromValue(sc=>sc.Simulations, simulations);
            //binder.BindMemberFromValue(sc=>sc.Years, years);

            //var bindingContext = new BindingContext( rootCommand.Parse( args ) );

            //var instance = (SimulationContext) binder.CreateInstance( bindingContext );

            var parsed = false;

            rootCommand.Handler = CommandHandler.Create<RawArgs>( raw =>
            {
                Years = raw.Y;
                Investments = raw.I;
                Simulations = raw.S;
                MaxAnnualInvestmentReturn = raw.R;
                MaxStdDevAnnualInvestmentReturn = raw.D;

                parsed = true;
            } );

            return rootCommand.Invoke( args ) == 0 && parsed;

            //return true;
        }
    }
}