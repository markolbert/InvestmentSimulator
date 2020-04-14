using System;
using System.Collections.Generic;
using System.IO;
using Alba.CsConsoleFormat;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoFacJ4JLogging;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Serilog.Events;
using static System.ConsoleColor;

namespace J4JSoftware.InvestmentSimulator
{
    class Program
    {
        private static readonly LineThickness _hdrThickness = new LineThickness( LineWidth.Single, LineWidth.Single );

        private static IServiceProvider _services;
        private static IJ4JLogger _logger;
        private static SimulationContext _context;
        private static Simulator _simulator;

        static void Main( string[] args )
        {
            _services = ConfigureServices();

            var loggerFactory = _services.GetRequiredService<IJ4JLoggerFactory>();
            _logger = loggerFactory.CreateLogger( typeof(Program) );

            _context = _services.GetRequiredService<SimulationContext>();
            
            if( !_context.Initialize( args ) )
            {
                Environment.ExitCode = 1;
                return;
            }

            OutputAssumptions();

            PressAnyKey("Press any key to run simulations");

            _simulator = _services.GetRequiredService<Simulator>();
            _simulator.Run( _context );

            OutputToExcel(_simulator);
        }

        private static IServiceProvider ConfigureServices()
        {
            var builder = new ContainerBuilder();

            var config = new J4JLoggerConfiguration
            {
                IncludeAssemblyName = false,
                IncludeSource = false
            };

            config.Channels.Add(new ConsoleChannel() { MinimumLevel = LogEventLevel.Information });
            config.Channels.Add(new FileChannel() { MinimumLevel = LogEventLevel.Verbose });

            builder.AddJ4JLogging( config );

            builder.RegisterType<SimulationContext>()
                .AsSelf();

            builder.RegisterType<Simulator>()
                .AsSelf();

            builder.RegisterType<ExcelExporter>()
                .AsSelf();

            return new AutofacServiceProvider(builder.Build());
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

        private static bool OutputToExcel( Simulator simulator )
        {
            var workbook = _services.GetRequiredService<ExcelExporter>();

            if( !workbook.Open() )
                return false;

            workbook.AddWorksheet( "assumptions" );
            OutputAssumptionsToExcel( workbook.ActiveWorksheet, simulator );

            workbook.AddWorksheet("statistics");
            OutputStatisticsToExcel(workbook.ActiveWorksheet, simulator);

            workbook.AddWorksheet("data");
            OutputDataToExcel( workbook.ActiveWorksheet, simulator);

            return workbook.Close();
        }

        private static void OutputAssumptionsToExcel( ExcelSheet sheet, Simulator simulator )
        {
            var table = new ExcelTable( sheet, _services.GetService<IJ4JLoggerFactory>(), TableOrientation.RowHeaders );

            table.AddHeaders( "Iterations to Run", "Number of Investments", "Time Span, Years",
                "Maximum Annual Rate of Return", "Maximum Standard Deviation in Rate of Return" );

            table.AddEntry( _context.Simulations, _context.Investments, _context.Years,
                _context.MaxAnnualInvestmentReturn, _context.MaxStdDevAnnualInvestmentReturn );

            var table2 = new ExcelTable(sheet, _services.GetService<IJ4JLoggerFactory>(), row: 7, col: 0);
            table2.AddHeaders("Investment #", "Inverse Gaussian Mean", "Inverse Gaussian Std Dev");

            for( var inv = 0; inv < simulator.Context.Investments; inv++ )
            {
                var ig = simulator.InverseInvestmentGaussians[ inv ];

                table2.AddEntry( inv, ig.Mean, ig.StdDev );
            }
        }

        private static void OutputStatisticsToExcel( ExcelSheet sheet, Simulator simulator )
        {
            var table1 = new ExcelTable(sheet, _services.GetService<IJ4JLoggerFactory>(), TableOrientation.RowHeaders);

            table1.AddHeaders("Overall Mean Return", "Overall Standard Deviation");
            table1.AddEntry(simulator.OverallMeanReturn, simulator.OverallStandardDeviation);

            var table2 = new ExcelTable(sheet, _services.GetService<IJ4JLoggerFactory>(), row:3, col: 0);
            table2.AddHeaders("Year", "Mean Portfolio Return", "Portfolio Standard Deviation");

            for( int year = 0; year < simulator.Context.Years; year++ )
            {
                table2.AddEntry( 
                    year, 
                    simulator.PortfolioReturn[ year ],
                    simulator.PortfolioStandardDeviation[ year ] );
            }
        }

        private static void OutputDataToExcel( ExcelSheet sheet, Simulator simulator )
        {
            var table = new ExcelTable(sheet, _services.GetService<IJ4JLoggerFactory>());

            table.AddHeaders("Simulation", "Investment #", "Year", "Rate of Return");

            foreach( var row in simulator.Results )
            {
                table.AddEntry( row.Simulation, row.Investment, row.Year, row.RateOfReturn );
            }

            table.AutoSize();
        }
    }
}
