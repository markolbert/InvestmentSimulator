using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using J4JSoftware.Logging;
using ObjectBinder;

namespace J4JSoftware.InvestmentSimulator
{
    public class SimulationContext : ObjectBindingModel
    {
        private readonly IJ4JLogger _logger;

        private int _years = 1;
        private int _investments = 1;
        private int _simulations = 1;
        private double _maxReturn = 0.2;
        private double _maxStdDevReturn = 0.2;

        public SimulationContext( IJ4JLoggerFactory loggerFactory )
        {
            _logger = loggerFactory?.CreateLogger( typeof(SimulationContext) ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );

            Betas = new BetaDistribution( loggerFactory );
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

        public BetaDistribution Betas { get; }

        public bool Initialize( string[] args )
        {
            var rootBinder = new ObjectBinder<SimulationContext>( new RootCommand( "Investment Simulator" ), this );

            rootBinder.AddOption( sc => sc.Years, "-y", "--years" )
                .Description( "years to simulate" )
                .DefaultValue( 10 )
                .Validator( OptionInRange<int>.GreaterThanEqual( 1 ) );

            rootBinder.AddOption( sc => sc.Investments, "-i", "--investments" )
                .Description( "investments to simulate" )
                .DefaultValue( 5 )
                .Validator( OptionInRange<int>.GreaterThanEqual( 1 ) );

            rootBinder.AddOption( sc => sc.Simulations, "-s", "--simulations" )
                .Description( "simulations to run" )
                .DefaultValue( 10 )
                .Validator( OptionInRange<int>.GreaterThanEqual( 1 ) );

            rootBinder.AddOption( sc => sc.MaxAnnualInvestmentReturn, "-r", "--maxReturn" )
                .Description( "maximum annual rate of return for an investment" )
                .DefaultValue( 0.2 )
                .Validator( OptionInRange<double>.GreaterThan( 0.0 ) );

            rootBinder.AddOption( sc => sc.MaxStdDevAnnualInvestmentReturn, "-d", "--maxStdDev" )
                .Description( "maximum standard deviation in the annual rate of return" )
                .DefaultValue( 0.2 )
                .Validator( OptionInRange<double>.GreaterThan( 0.0 ) );

            Parse( rootBinder, args );

            return !HelpRequested;
        }
    }
}