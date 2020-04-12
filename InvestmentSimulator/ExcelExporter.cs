using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.InvestmentSimulator
{
    public class ExcelExporter
    {
        private readonly IJ4JLoggerFactory _loggerFactory;
        private readonly IJ4JLogger _logger;

        private FileStream _excelStream;
        private XSSFWorkbook _workbook;

        public ExcelExporter( 
            IJ4JLoggerFactory loggerFactory 
            )
        {
            _loggerFactory = loggerFactory ?? throw new NullReferenceException( nameof(loggerFactory) );
            _logger = _loggerFactory.CreateLogger( typeof(ExcelExporter) );
        }

        public bool IsValid { get; private set; }
        public List<ExcelSheet> Worksheets { get; } = new List<ExcelSheet>();

        public ExcelSheet ActiveWorksheet
        {
            get
            {
                if( Worksheets.Count == 0 )
                {
                    var mesg = "No worksheets are defined";
                    _logger.Error( mesg );

                    throw new IndexOutOfRangeException( mesg );
                }

                return Worksheets.Last( x => true );
            }
        }

        public ExcelSheet this[ string name ]
        {
            get
            {
                var retVal =
                    Worksheets.FirstOrDefault( w => w.Sheet.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) );

                if( retVal == null )
                {
                    var mesg = $"Could not find worksheet '{name}'";
                    _logger.Error(mesg  );

                    throw new ArgumentException( mesg );
                }

                return retVal;
            }
        }

        public bool Open()
        {
            while( true )
            {
                Console.Write( "\n\nEnter file name, press return (blank to skip): " );
                var fileName = Console.ReadLine();

                if( string.IsNullOrEmpty( fileName ) )
                    break;

                fileName += ".xlsx";
                var path = Path.Combine( Environment.CurrentDirectory, fileName );

                try
                {
                    _excelStream = File.Create( path );
                    break;
                }
                catch( Exception e )
                {
                    var mesg = $"Invalid valid name '{path}'";
                    _logger.Error( mesg );
                    Console.WriteLine( mesg );
                }
            }

            IsValid = _excelStream != null;

            if( IsValid )
            {
                _workbook = new XSSFWorkbook();
                _logger.Information( $"Opened Excel file '{_excelStream.Name}', created workbook" );
            }
            else _workbook = null;

            return IsValid;
        }

        public bool AddWorksheet( string name )
        {
            if( string.IsNullOrEmpty( name ) )
            {
                _logger.Error( $"Undefined or empty worksheet name" );
                return false;
            }

            if( Worksheets.Any( w => w.Sheet.SheetName.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
            {
                _logger.Error( $"Duplicate worksheet name '{name}'" );
                return false;
            }

            Worksheets.Add( new ExcelSheet( _workbook.CreateSheet( name ), _loggerFactory ) );

            return true;
        }

        public bool Close()
        {
            if( !IsValid )
            {
                _logger.Error( $"{nameof(ExcelExporter)} is invalid, can't close workbook" );
                return false;
            }

            _workbook.Write( _excelStream );
            _workbook = null;

            return true;
        }
    }
}
