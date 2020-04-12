using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.InvestmentSimulator
{
    public class ExcelTable
    {
        private readonly IJ4JLogger _logger;

        public ExcelTable(
            ExcelSheet excelSheet,
            IJ4JLoggerFactory loggerFactory
        )
        {
            _logger = loggerFactory?.CreateLogger( typeof(ExcelTable) ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );

            ExcelSheet = excelSheet ?? throw new NullReferenceException( nameof(excelSheet) );

            UpperLeftRow = ExcelSheet.ActiveRowNumber;
            UpperLeftColumn = ExcelSheet.ActiveColumnNumber;
        }

        public ExcelSheet ExcelSheet { get; }
        public int UpperLeftRow { get; }
        public int UpperLeftColumn { get; }

        public int NumColumns { get; private set; }
        public int NumRows { get; private set; }

        public void AddColumnHeader( string header )
        {
            ExcelSheet.MoveTo( UpperLeftRow, UpperLeftColumn + NumColumns );
            ExcelSheet.ActiveCell.SetCellValue(header);

            NumColumns++;
        }

        public void AddRow( params object[] values )
        {
            AddRow( new List<object>( values ) );
        }

        public void AddRow( List<object> values )
        {
            ExcelSheet.MoveTo( UpperLeftRow + NumRows + 1, UpperLeftColumn );

            for( var idx = 0; idx < ( values.Count > NumColumns ? values.Count : NumColumns ); idx++ )
            {
                if( idx < values.Count )
                    ExcelSheet.ActiveCell.SetValue( values[idx] );

                ExcelSheet.Move( 0, 1 );
            }

            NumRows++;
            ExcelSheet.Move( 0, -values.Count );
        }
    }
}