using System;
using Alba.CsConsoleFormat;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using static System.ConsoleColor;

namespace InvestmentSimulator
{
    class Program
    {
        private static readonly LineThickness _hdrThickness = new LineThickness( LineWidth.Single, LineWidth.Single );

        private static IJ4JLogger<Program> _logger;
        private static SimulationContext _context;

        static void Main( string[] args )
        {
            _logger = ServiceProvider.Instance.GetRequiredService<IJ4JLogger<Program>>();

            _context = ServiceProvider.Instance.GetRequiredService<SimulationContext>();
            
            if( !_context.Initialize( args ) )
            {
                Environment.ExitCode = 1;
                return;
            }

            OutputAssumptions();

            PressAnyKey("Press any key to run simulations");
        }

        private static void PressAnyKey( string prompt = "Press any key to continue" )
        {
            Console.Write($"\n{prompt}: ");
            Console.ReadKey(false);
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
    }
}
