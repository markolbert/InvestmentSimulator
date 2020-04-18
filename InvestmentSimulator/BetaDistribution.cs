using System;
using J4JSoftware.Logging;
using MathNet.Numerics.Distributions;

namespace J4JSoftware.InvestmentSimulator
{
    public class BetaDistribution
    {
        private readonly IJ4JLogger _logger;

        private double _alpha;
        private double _beta;
        private Beta _betaDist;

        public BetaDistribution( IJ4JLoggerFactory loggerFactory )
        {
            _logger = loggerFactory?.CreateLogger( this.GetType() ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );
        }

        public double Alpha
        {
            get => _alpha;

            set
            {
                _alpha = value;
                _betaDist = null;
            }
        }

        public double Beta
        {
            get=> _beta;

            set
            {
                _beta = value;
                _betaDist = null;
            }
        }

        public double Minimum { get; set; } = -1.0;
        public double Maximum { get; set; } = 1.0;

        public Beta Distribution => _betaDist ??= new Beta( Alpha, Beta );

        public bool SetRange(double min, double max)
        {
            if (Math.Abs(min - max) < 0.0001)
            {
                _logger.Error($"{nameof(min)} and {nameof(max)} are too similar (<=0.0001)");
                return false;
            }

            if (min > max)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            Minimum = min;
            Maximum = max;

            return true;
        }
    }
}