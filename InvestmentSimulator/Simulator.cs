using J4JSoftware.Logging;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.InvestmentSimulator
{
    public struct SimulationResult
    {
        public int Simulation { get; set; }
        public int Investment { get; set; }
        public int Year { get; set; }
        public double RateOfReturn { get; set; }
    }

    public class Simulator
    {
        private readonly IJ4JLogger _logger;

        public Simulator( IJ4JLoggerFactory loggerFactory )
        {
            _logger = loggerFactory?.CreateLogger( typeof(Simulator) ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );
        }

        public bool IsValid { get; private set; }
        public SimulationContext Context { get; private set; }
        public double[,,] Values { get; private set; }
        public List<SimulationResult> Results { get; private set; }
        public InverseGaussian[] InverseInvestmentGaussians { get; private set; }
        public double[] PortfolioReturnsByYear { get; private set; }
        public double[] PortfolioStandardDeviationsByYear { get; private set; }
        public double[] SimulationGeometricReturns { get; private set; }
        public double OverallGeometricReturnMean => SimulationGeometricReturns?.Average() ?? -1.0;
        public double OverallGeometricReturnStandardDeviation => SimulationGeometricReturns == null
            ? -1.0
            : ArrayStatistics.PopulationStandardDeviation( SimulationGeometricReturns );

        public double[,] RawGeometricReturns { get; private set; }

        public bool Run( SimulationContext context )
        {
            IsValid = false;

            if( context == null )
            {
                _logger.Error( $"Undefined {nameof(context)}" );
                return false;
            }

            Context = context;
            Values = new double[Context.Years, Context.Investments, Context.Simulations];

            InverseInvestmentGaussians = new InverseGaussian[Context.Investments];

            var random = new Random();

            for( var inv = 0; inv < Context.Investments; inv++ )
            {
                InverseInvestmentGaussians[ inv ] = new InverseGaussian(
                    Context.MeanMarketReturn * random.NextDouble(),
                    Context.StdDevMarketReturn * random.NextDouble()
                );
            }

            CalculateSimulations();
            SummarizeRawGeometricReturns();
            SummarizePortfolioReturnsByYear();
            CalculatePortfolioReturnStandardDeviationsByYear();
            CalculateGeometricMeanReturnsBySimulation();

            IsValid = true;

            return true;
        }

        private void CalculateSimulations()
        {
            Console.Write("Calculating simulations...");

            Results = new List<SimulationResult>();

            for( var inv = 0; inv < Context.Investments; inv++ )
            {
                var year = 0;
                var sim = 0;

                _logger.Information(
                    $"Generating {Context.Simulations:n0} simulations of {Context.Years:n0} years of returns for investment #{( inv + 1 ):n0}" );

                foreach( var sample in InverseInvestmentGaussians[ inv ].Samples()
                    .Take( Context.Years * Context.Simulations ) )
                {
                    Values[ year, inv, sim ] = sample;

                    Results.Add( new SimulationResult
                        { Investment = inv, RateOfReturn = sample, Simulation = sim, Year = year } );

                    sim++;

                    if( sim < Context.Simulations )
                        continue;

                    sim = 0;
                    year++;

                    if( year >= Context.Years )
                        break;
                }
            }

            Console.WriteLine("done.");
        }

        private void SummarizeRawGeometricReturns()
        {
            Console.Write("Summarizing raw geometric returns by simulation and investment...");

            RawGeometricReturns = new double[Context.Simulations, Context.Investments];

            for (var sim = 0; sim < Context.Simulations; sim++)
            {
                for (var inv = 0; inv < Context.Investments; inv++)
                {
                    RawGeometricReturns[sim, inv] = 1.0;

                    for (var year = 0; year < Context.Years; year++)
                    {
                        RawGeometricReturns[sim, inv] *= (1 + Values[year, inv, sim]);
                    }
                }
            }

            Console.WriteLine("done.");
        }

        private void SummarizePortfolioReturnsByYear()
        {
            Console.Write("Summarizing portfolio returns by year...");

            PortfolioReturnsByYear = new double[Context.Years];
            var invSim = Context.Investments * Context.Simulations;

            for (var year = 0; year < Context.Years; year++)
            {
                var cumlYearReturn = 0.0;

                for (var inv = 0; inv < Context.Investments; inv++)
                {
                    for (var sim = 0; sim < Context.Simulations; sim++)
                    {
                        cumlYearReturn += Values[year, inv, sim];
                    }
                }

                PortfolioReturnsByYear[year] = cumlYearReturn / invSim;
            }

            Console.WriteLine("done.");
        }

        private void CalculatePortfolioReturnStandardDeviationsByYear()
        {
            Console.Write("Calculating standard deviations of portfolio returns by year...");

            PortfolioStandardDeviationsByYear = new double[Context.Years];
            var invSim = Context.Investments * Context.Simulations;

            for (var year = 0; year < Context.Years; year++)
            {
                var cumlInvVariance = 0.0;

                for (var inv = 0; inv < Context.Investments; inv++)
                {
                    for (var sim = 0; sim < Context.Simulations; sim++)
                    {
                        var deltaFromAvg = Values[year, inv, sim] - PortfolioReturnsByYear[year];

                        cumlInvVariance += deltaFromAvg * deltaFromAvg;
                    }
                }

                PortfolioStandardDeviationsByYear[year] = Math.Sqrt(cumlInvVariance / invSim);
            }

            Console.WriteLine("done.");
        }

        private void CalculateGeometricMeanReturnsBySimulation()
        {
            Console.Write("Calculating standard deviations of portfolio returns by year...");

            SimulationGeometricReturns = new double[Context.Simulations];

            for (var sim = 0; sim < Context.Simulations; sim++)
            {
                SimulationGeometricReturns[sim] = 0.0;

                for (var inv = 0; inv < Context.Investments; inv++)
                {
                    var invTotalReturn = 1.0;

                    for (var year = 0; year < Context.Years; year++)
                    {
                        invTotalReturn *= (1 + Values[year, inv, sim]);
                    }

                    SimulationGeometricReturns[sim] += invTotalReturn;
                }
            }

            // convert to geometric means
            for (var sim = 0; sim < Context.Simulations; sim++)
            {
                SimulationGeometricReturns[ sim ] = Math.Pow( 
                    SimulationGeometricReturns[ sim ] / Context.Investments,
                    1.0 / Context.Years ) - 1;
            }

            Console.WriteLine("done.");
        }
    }
}
