using System;
using System.IO;
using Alba.CsConsoleFormat;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using static System.ConsoleColor;

namespace J4JSoftware.InvestmentSimulator
{
    class Program
    {
        private static readonly LineThickness _hdrThickness = new LineThickness( LineWidth.Single, LineWidth.Single );

        private static IJ4JLogger _logger;
        private static SimulationContext _context;
        private static Simulator _simulator;

        static void Main( string[] args )
        {
            var loggerFactory = ServiceProvider.Instance.GetRequiredService<IJ4JLoggerFactory>();
            _logger = loggerFactory.CreateLogger( typeof(Program) );

            _context = ServiceProvider.Instance.GetRequiredService<SimulationContext>();
            
            if( !_context.Initialize( args ) )
            {
                Environment.ExitCode = 1;
                return;
            }

            OutputAssumptions();

            PressAnyKey("Press any key to run simulations");

            _simulator = ServiceProvider.Instance.GetRequiredService<Simulator>();
            _simulator.Run( _context );

            var junk = _simulator.OverallMeanReturn;
            junk = _simulator.OverallStandardDeviation;
            var crap = _simulator.PortfolioReturn;
            crap = _simulator.PortfolioStandardDeviation;
        }

        private static void PressAnyKey( string prompt = "Press any key to continue" )
        {
            Console.Write($"\n{prompt}: ");
            Console.ReadKey(false);
            Console.WriteLine("\n");
        }

        private static void OutputAssumptions()
        {
            Console.Clear();
            Console.SetCursorPosition( 0, 0 );

            var doc = new Alba.CsConsoleFormat.Document(
                new Span( "Investment Simulator" ) { Color = Yellow },
                new Grid()
                {
                    Color = ConsoleColor.Gray,
                    Columns = { GridLength.Auto, GridLength.Auto },
                    Children =
                    {
                        new Cell( "Iterations to run" ) { Stroke = _hdrThickness, Align = Align.Left },
                        new Cell( $"{_context.Simulations:n0}" ) { Stroke = _hdrThickness, Align = Align.Right },
                        new Cell( "Number of Investments" ) { Stroke = _hdrThickness, Align = Align.Left },
                        new Cell( $"{_context.Investments:n0}" ) { Stroke = _hdrThickness, Align = Align.Right },
                        new Cell( "Time Span, Years" ) { Stroke = _hdrThickness, Align = Align.Left },
                        new Cell( $"{_context.Years:n0}" ) { Stroke = _hdrThickness, Align = Align.Right },
                        new Cell( "Maximum Annual Rate of Return" ) { Stroke = _hdrThickness, Align = Align.Left },
                        new Cell( $"{_context.MaxAnnualInvestmentReturn*100:n1}%" ) { Stroke = _hdrThickness, Align = Align.Right },
                        new Cell( "Maximum Standard Deviation in Rate of Return" ) { Stroke = _hdrThickness, Align = Align.Left },
                        new Cell( $"{_context.MaxStdDevAnnualInvestmentReturn*100:n1}%" ) { Stroke = _hdrThickness, Align = Align.Right },
                    }
                } );

            ConsoleRenderer.RenderDocument( doc );
        }

        private static bool OutputToExcel( )
        {
            var workbook = ServiceProvider.Instance.GetRequiredService<ExcelExporter>();

            if( !workbook.Open() )
                return false;

            workbook.AddWorksheet( "assumptions" );

            OutputAssumptionsToExcel( workbook.ActiveWorksheet );

            return true;
        }

        private static void OutputAssumptionsToExcel( ExcelSheet sheet )
        {

        }
    }
}
