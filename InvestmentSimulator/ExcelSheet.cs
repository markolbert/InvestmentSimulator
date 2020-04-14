using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.InvestmentSimulator
{
    public class ExcelSheet
    {
        private readonly IJ4JLogger _logger;
        private readonly IJ4JLoggerFactory _loggerFactory;
        private readonly List<IRow> _rows = new List<IRow>();
        private readonly List<ICell> _cells = new List<ICell>();

        public ExcelSheet( 
            ISheet sheet,
            IJ4JLoggerFactory loggerFactory
        )
        {
            _loggerFactory = loggerFactory ?? throw new NullReferenceException( nameof(loggerFactory) );
            _logger = _loggerFactory.CreateLogger( typeof(ExcelSheet) );

            Sheet = sheet ?? throw new NullReferenceException( nameof(sheet) );
        }

        public ISheet Sheet { get; }
        public int ActiveRowNumber { get; private set; }
        public int ActiveColumnNumber { get; private set; }

        public ICell this[ int row, int col ]
        {
            get
            {
                if( row < 0 )
                {
                    var mesg = $"Invalid {nameof(row)} ({row})";
                    _logger.Error( mesg );

                    throw new IndexOutOfRangeException( mesg );
                }

                if( col < 0 )
                {
                    var mesg = $"Invalid {nameof( col )} ({col})";
                    _logger.Error( mesg );

                    throw new IndexOutOfRangeException( mesg );
                }

                var theRow = _rows.FirstOrDefault( r => r.RowNum == row );

                if( theRow == null )
                {
                    theRow = Sheet.CreateRow( row );
                    _rows.Add(theRow  );
                }

                var retVal = _cells.FirstOrDefault( c => c.RowIndex == row && c.ColumnIndex == col );

                if( retVal == null )
                {
                    retVal = theRow.CreateCell( col );
                    _cells.Add(retVal  );
                }

                return retVal;
            }
        }

        public IRow ActiveRow
        {
            get
            {
                var retVal = _rows.FirstOrDefault( r => r.RowNum == ActiveRowNumber );

                if( retVal == null )
                {
                    retVal = Sheet.CreateRow( ActiveRowNumber );
                    _rows.Add( retVal );
                }

                return retVal;
            }
        }

        public ICell ActiveCell
        {
            get
            {
                var row = ActiveRow;

                var retVal = _cells.FirstOrDefault( c => c.RowIndex == row.RowNum
                                                         && c.ColumnIndex == ActiveColumnNumber );

                if( retVal == null )
                {
                    retVal = row.CreateCell( ActiveColumnNumber );
                    _cells.Add( retVal );
                }

                return retVal;
            }
        }

        public ExcelSheet MoveTo( int row, int col )
        {
            if( row < 0 )
            {
                _logger.Error( $"{nameof(row)} cannot be < 0 ({row})" );
                return this;
            }

            if( col < 0 )
            {
                _logger.Error( $"{nameof( col )} cannot be < 0 ({col})" );
                return this;
            }

            ActiveRowNumber = row;
            ActiveColumnNumber = col;

            return this;
        }

        public ExcelSheet Move( int rows, int cols )
        {
            if( rows + ActiveRowNumber < 0 )
            {
                _logger.Error( $"Cannot move before row 0 ({rows})" );
                return this;
            }

            if( cols + ActiveColumnNumber < 0 )
            {
                _logger.Error( $"Cannot move before column 0 ({cols})" );
                return this;
            }

            ActiveRowNumber += rows;
            ActiveColumnNumber += cols;

            return this;
        }

        public ExcelSheet AddNameValueRow( string name, object value )
        {
            ActiveCell.SetCellValue(name);
            ActiveColumnNumber++;

            ActiveCell.SetValue( value );

            ActiveColumnNumber--;
            ActiveRowNumber++;

            return this;
        }
    }
}