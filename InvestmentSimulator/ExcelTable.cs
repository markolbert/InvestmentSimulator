using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.InvestmentSimulator
{
    public enum TableOrientation
    {
        ColumnHeaders,
        RowHeaders
    }

    public class ExcelTable
    {
        private readonly IJ4JLogger _logger;
        private readonly TableOrientation _orientation;

        public ExcelTable(
            ExcelSheet excelSheet,
            IJ4JLoggerFactory loggerFactory,
            TableOrientation orientation = TableOrientation.ColumnHeaders,
            int? row = null,
            int? col = null
        )
        {
            _logger = loggerFactory?.CreateLogger( typeof(ExcelTable) ) ??
                      throw new NullReferenceException( nameof(loggerFactory) );

            _orientation = orientation;

            ExcelSheet = excelSheet ?? throw new NullReferenceException( nameof(excelSheet) );

            if( row.HasValue && col.HasValue )
            {
                UpperLeftColumn = col.Value;
                UpperLeftRow = row.Value;
            }
            else
            {
                UpperLeftRow = ExcelSheet.ActiveRowNumber;
                UpperLeftColumn = ExcelSheet.ActiveColumnNumber;
            }
        }

        public ExcelSheet ExcelSheet { get; }
        public int UpperLeftRow { get; }
        public int UpperLeftColumn { get; }

        public int NumHeaders { get; private set; }
        public int NumEntries { get; private set; }

        public void AutoSize()
        {
            var columns = _orientation == TableOrientation.ColumnHeaders ? NumHeaders : NumEntries;

            for( var column = 0; column < columns; column++ )
            {
                ExcelSheet.Sheet.AutoSizeColumn( column );
            }
        }

        public void AddHeader( string header )
        {
            ExcelSheet.MoveTo(
                UpperLeftRow + ( _orientation == TableOrientation.ColumnHeaders ? 0 : NumHeaders ),
                UpperLeftColumn + ( _orientation == TableOrientation.RowHeaders ? 0 : NumHeaders )
            );

            ExcelSheet.ActiveCell.SetCellValue( header );

            NumHeaders++;
        }

        public void AddHeaders( params string[] headers )
        {
            foreach( var header in headers )
            {
                AddHeader( header );
            }
        }

        public void AddEntry<TEntity>( TEntity entity )
        {
            if( entity == null )
                return;

            var values = typeof(TEntity).GetProperties()
                .Select( p => p.GetValue( entity ) )
                .ToArray();

            if( values.Length > 0 )
                AddEntry( values );
        }

        public void AddEntry( params object[] values )
        {
            AddEntry( new List<object>( values ) );
        }

        public void AddEntry( List<object> values )
        {
            ExcelSheet.MoveTo(
                UpperLeftRow + ( _orientation == TableOrientation.RowHeaders ? 0 : NumEntries + 1),
                UpperLeftColumn + ( _orientation == TableOrientation.ColumnHeaders ? 0 : NumEntries + 1 )
            );

            for ( var idx = 0; idx < ( values.Count > NumHeaders ? values.Count : NumHeaders ); idx++ )
            {
                if( idx < values.Count )
                    ExcelSheet.ActiveCell.SetValue( values[idx] );

                ExcelSheet.Move(
                    _orientation == TableOrientation.RowHeaders ? 1 : 0,
                    _orientation == TableOrientation.ColumnHeaders ? 1 : 0
                );
            }

            NumEntries++;

            ExcelSheet.Move(
                _orientation == TableOrientation.RowHeaders ? -values.Count : 0,
                _orientation == TableOrientation.ColumnHeaders ? -values.Count : 0
            );
        }
    }
}