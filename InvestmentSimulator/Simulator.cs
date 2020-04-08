using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using MathNet.Numerics.Distributions;

namespace InvestmentSimulator
{
    public class Simulator
    {
        private readonly IJ4JLogger<Simulator> _logger;

        private SimulationContext _context;
        private double[,,] _values;
        private InverseGaussian[] _invGaussians;

        public Simulator( IJ4JLogger<Simulator> logger )
        {
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );
        }

        public double[,,] Values
        {
            get
            {
                if( _values == null )
                {
                    _logger.Error( $"{nameof(Values)} is undefined" );
                    return new double[0, 0, 0];
                }

                return _values;
            }
        }

        public InverseGaussian[] InverseInvestmentGaussians
        {
            get
            {
                if( _context == null )
                {
                    _logger.Error( $"{nameof(InverseInvestmentGaussians)} is undefined" );
                    return new InverseGaussian[0];
                }

                return _invGaussians;
            }
        }

        public double[] PortfolioReturn
        {
            get
            {
                if( _values == null || _context == null )
                {
                    _logger.Error( $"{nameof( PortfolioReturn )} undefined" );
                    return new double[0];
                }

                return Enumerable.Range( 0, _context.Years )
                    .Select( y =>
                    {
                        var retVal = 0.0;

                        for( var inv = 0; inv < _context.Investments; inv++ )
                        {
                            for( var sim = 0; sim < _context.Simulations; sim++ )
                            {
                                retVal += _values[ y, inv, sim ];
                            }
                        }

                        return retVal / (_context.Investments * _context.Simulations);
                    } )
                    .ToArray();
            }
        }

        public double[] PortfolioStandardDeviation
        {
            get
            {
                if( _values == null || _context == null )
                {
                    _logger.Error( $"{nameof( PortfolioStandardDeviation )} undefined" );
                    return new double[ 0 ];
                }

                var means = PortfolioReturn;

                return Enumerable.Range( 0, _context.Years )
                    .Select( y =>
                    {
                        var retVal = 0.0;

                        for( var inv = 0; inv < _context.Investments; inv++ )
                        {
                            var simAvg = 0.0;

                            for( var sim = 0; sim < _context.Simulations; sim++ )
                            {
                                simAvg += _values[ y, inv, sim ];
                            }

                            var delta = ( simAvg - means[ y ] ) ;
                            retVal += delta * delta;
                        }

                        return Math.Sqrt( retVal / _context.Investments );
                    } )
                    .ToArray();
            }
        }

        public double OverallMeanReturn
        {
            get
            {
                if( _values == null || _context == null )
                {
                    _logger.Error( $"{nameof( OverallMeanReturn )} undefined" );
                    return 0.0;
                }

                var retVal = 1.0;

                foreach( var mean in PortfolioReturn )
                {
                    retVal *= ( 1 + mean );
                }

                return retVal - 1;
            }
        }

        public double OverallStandardDeviation
        {
            get
            {
                if( _values == null || _context == null )
                {
                    _logger.Error( $"{nameof( OverallStandardDeviation )} undefined" );
                    return 0.0;
                }

                var retVal = 0.0;
                var overallMean = OverallMeanReturn;

                for( var sim = 0; sim < _context.Simulations; sim++ )
                {
                    var simReturn = 1.0;

                    for( var year = 0; year < _context.Years; year++ )
                    {
                        var portfolioReturn = 0.0;

                        for( var inv = 0; inv < _context.Investments; inv++ )
                        {
                            portfolioReturn += _values[ year, inv, sim ];
                        }

                        simReturn *= ( 1 + portfolioReturn / _context.Investments );
                    }

                    var delta = ( simReturn - 1 ) - overallMean;
                    retVal += delta * delta;
                }

                return Math.Sqrt( retVal / _context.Simulations );
            }
        }

        public bool Run( SimulationContext context )
        {
            if( context == null )
            {
                _logger.Error( $"Undefined {nameof(context)}" );
                return false;
            }

            _context = context;
            _values = new double[_context.Years, _context.Investments, _context.Simulations];

            _invGaussians = new InverseGaussian[_context.Investments];

            var random = new Random();

            for( var inv = 0; inv < _context.Investments; inv++ )
            {
                _invGaussians[ inv ] = new InverseGaussian(
                    _context.MaxAnnualInvestmentReturn * random.NextDouble(),
                    _context.MaxStdDevAnnualInvestmentReturn * random.NextDouble()
                );
            }

            for( var inv = 0; inv < _context.Investments; inv++ )
            {
                var year = 0;
                var sim = 0;

                _logger.Information(
                    $"Generating {_context.Simulations:n0} simulations of {_context.Years:n0} years of returns for investment #{inv:n0}" );

                foreach( var sample in _invGaussians[ inv ].Samples()
                    .Take( _context.Years * _context.Investments ) )
                {
                    _values[ year, inv, sim ] = sample;

                    sim++;

                    if( sim < _context.Simulations ) 
                        continue;

                    year++;

                    if( year >= _context.Years )
                        break;
                }
            }

            _logger.Information( "Simulations completed" );

            return true;
        }
    }
}
