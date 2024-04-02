
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.IO.IsolatedStorage;

using DEBUG = DebugUtils;
using BitBarons.Util; // old: Log

namespace Sunbow.Util.IO
{

    /// <summary>
    /// The Table class stores csv-table data. It can contain any parsable data content.
    /// </summary>
    public class Table
    {
        #region Static


        /// <summary>
        /// iterates through all strings inside the passed iterator and checks if it can be parsed to the given type
        /// </summary>
        /// <typeparam name="T">the type to check</typeparam>
        /// <param name="cellIterator">the iterator. In common myTable.GetRowCellIterator, myTable.GetColumnCellIterator or myTable.GetTableIterator is passed here</param>
        /// <returns>true, if all the cells are valid</returns>
        public static bool CheckCells<T>(IEnumerable<string> cellIterator)
        {
            return CheckCells<T>(cellIterator, Parser.TryParse<T>);
        }
        /// <summary>
        /// iterates through all strings inside the passed iterator and checks if it can be parsed to the given type
        /// </summary>
        /// <typeparam name="T">the type to check</typeparam>
        /// <param name="cellIterator">the iterator. In common myTable.GetRowCellIterator or myTable.GetColumnCellIterator is passed here</param>
        /// <param name="checkMethod">the method which is used to check if the given type is of the correct type</param>
        /// <returns>true, if all the cells are valid</returns>
        public static bool CheckCells<T>(IEnumerable<string> cellIterator, TryParse<T> checkMethod)
        {
            bool result = true;
            T tmp;
            foreach (string s in cellIterator)
            {
                result = result && checkMethod(s, out tmp);
            }
            return result;
        }

        public static T GetMaximum<T>(IEnumerable<string> cellIterator) where T : IComparable<T>
        {
            T result = default(T); 
            bool changed = false;
            foreach (string s in cellIterator)
            {
                T tmp;
                if (Parser.TryParse<T>(s, out tmp))
                {
                    if (!changed || tmp.CompareTo(result) > 0)
                    {
                        changed = true;
                        result = tmp;
                    }
                }
            }
            return result;
        }
        public static T GetMinimum<T>(IEnumerable<string> cellIterator) where T : IComparable<T>
        {
            T result = default(T);
            bool changed = false;
            foreach (string s in cellIterator)
            {
                T tmp;
                if (Parser.TryParse<T>(s, out tmp))
                {
                    if (!changed || tmp.CompareTo(result) < 0)
                    {
                        changed = true;
                        result = tmp;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Variables
        // -- VARIABLES --

        public CSVSetting Settings;// { get; private set; }

        /// <summary>the cells of the table. It is not in the common order... it is: <code>cells[row][column]</code></summary>
        List<List<string>> cells = new List<List<string>>();

        /// <summary>
        /// Gets or sets the string in the given cell
        /// </summary>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <returns>the string which is in the given cell</returns>
        public string this[int column, int row] { get { return cells[row][column]; } set { Set(column, row, value); } }
        /// <summary>
        /// Gets or sets the string in the given cell
        /// </summary>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row of the cell</param>
        /// <returns>the string which is in the given cell</returns>
        public string this[string column, int row] { get { return cells[row][columnHeaders.IndexOf(column)]; } set { Set(columnHeaders.IndexOf(column), row, value); } }
        /// <summary>
        /// Gets or sets the string in the given cell
        /// </summary>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <returns>the string which is in the given cell</returns>
        public string this[int column, string row] { get { return cells[rowHeaders.IndexOf(row)][column]; } set { Set(column, rowHeaders.IndexOf(row), value); } }
        /// <summary>
        /// Gets or sets the string in the given cell
        /// </summary>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <returns>the string which is in the given cell</returns>
        public string this[string column, string row] { get { return cells[rowHeaders.IndexOf(row)][columnHeaders.IndexOf(column)]; } set { Set(columnHeaders.IndexOf(column), rowHeaders.IndexOf(row), value); } }


        /// <summary>
        /// Gets an Array of all headers which identifies the rows
        /// </summary>
        public string[] RowHeaders { get { return rowHeaders.ToArray(); } }
        /// <summary>
        /// Gets an Array of all headers which identifies the column
        /// </summary>
        public string[] ColumnHeaders { get { return columnHeaders.ToArray(); } }

        // lists which contain the headers (= stringID) for the rows / columns. They are used to find the correct indices in the table
        List<string> rowHeaders = new List<string>();
        List<string> columnHeaders; // just a reference to the header-nested-list inside cells

       // int headerRow, headerColumn; // the index of the header's (= stringID) row / column 

        /// <summary>The file name of the csv file which this table is loaded from</summary>
        public string FileName { get { return fileName; } set { fileName = value; } }
        string fileName;

        /// <summary>the number of rows in the table</summary>
        public int Rows { get { return rowHeaders.Count; } }
        /// <summary>the number of columns in the table</summary>
        public int Columns { get { return columnHeaders.Count; } }

        
        #endregion // Variables

        #region Constructors
        // -- CONSTRUCT --
        /// <summary>
        /// Loads a new Table from a file
        /// </summary>
        /// <param name="file">the full (relative) path with filename where the table file is located</param>
        public Table(string file)
            : this(file, CSVSetting.UniqueHeaders, true)
        {
        }
        /// <summary>
        /// Loads a new Table from a file
        /// </summary>
        /// <param name="file">the full (relative) path with filename where the table file is located</param>
        public Table(string file, bool isolatedStorage)
            : this(file, CSVSetting.UniqueHeaders, isolatedStorage)
        {
        }
        /// <summary>
        /// Loads a new Table from a file
        /// </summary>
        /// <param name="file">the full (relative) path with filename where the table file is located</param>
        /// <param name="settings">the csv settings which specifies the format of the csv table</param>
        public Table(string file, CSVSetting settings, bool isolatedStorage)
        {
            this.fileName = file;
            this.Settings = settings; 

            LoadFile(file, settings, isolatedStorage);

            UpdateHeaders(settings.EnsureUniqueRowHeaders, settings.EnsureUniqueColumnHeaders);
        }

        public Table(Stream stream, string fileName)
            : this(stream, fileName, CSVSetting.UniqueHeaders)
        {
        }

        public Table(Stream stream, string fileName, CSVSetting settings)
        {
            this.fileName = fileName;
            this.Settings = settings;

            LoadFromStream(stream, settings);

            UpdateHeaders(settings.EnsureUniqueRowHeaders, settings.EnsureUniqueColumnHeaders);
        }
        public Table(string input, string fileName)
            : this(input, fileName, CSVSetting.UniqueHeaders)
        {
        }

        public Table(string input, string fileName, CSVSetting settings)
        {
            this.fileName = fileName;
            this.Settings = settings;

            LoadFromString(input, settings);

            UpdateHeaders(settings.EnsureUniqueRowHeaders, settings.EnsureUniqueColumnHeaders);
            
        }

        /// <summary>
        /// Creates a new Table with the given dimension where every cell is empty
        /// </summary>
        /// <param name="fileName">the name of the table. this name is used when you save the table without passing a fileName to the <c>Save</c> method</param>
        /// <param name="columns">the number of columns in the table</param>
        /// <param name="rows">the number of rows in the table</param>
        public Table(string fileName, int columns, int rows)
            : this(fileName, columns, rows, CSVSetting.UniqueHeaders)
        {
        }
        public Table(string fileName, int columns, int rows, CSVSetting settings)
        {
            this.Settings = settings;
            this.fileName = fileName;

            for (int row = 0; row < rows; row++)
            {
                cells.Add(new List<string>());

                for (int col = 0; col < columns; col++)
                {
                    cells[row].Add("N/A");
                }
            }

            if (columns > 0 && rows > 0)
                UpdateHeaders(false, false);
        }

        #region XNA Loading
        //internal void FillWithContentData(ContentReader input)
        //{
        //    // WRITER CODE
        //    //output.Write(value.Settings.ColumnHeaderIndex);
        //    //output.Write(value.Settings.RowHeaderIndex);
        //    //output.Write(value.Settings.EnsureUniqueColumnHeaders);
        //    //output.Write(value.Settings.EnsureUniqueRowHeaders);
        //    //output.Write(value.Settings.CommentIdentifier);
        //    //output.Write(value.Settings.ColumnSeparator);
        //    //output.Write(value.Settings.StringIdentifier);
        //    //output.Write(value.Columns);
        //    //output.Write(value.Rows);
        //    //foreach (string cell in value.GetTableIterator(true))
        //    //{
        //    //    output.Write(cell);
        //    //}

        //    this.Settings = new CSVSetting()
        //    {
        //        ColumnHeaderIndex = input.ReadInt32(),
        //        RowHeaderIndex = input.ReadInt32(),
        //        EnsureUniqueColumnHeaders = input.ReadBoolean(),
        //        EnsureUniqueRowHeaders = input.ReadBoolean(),
        //        CommentIdentifier = input.ReadString()
        //    };
        //    this.Settings.SetIdentChars(input.ReadChar(), input.ReadChar());

        //    int columns = input.ReadInt32();
        //    int rows = input.ReadInt32();

        //    cells.Clear();
        //    for (int r = 0; r < rows; r++)
        //    {
        //        cells.Add(new List<string>());
        //        for (int c = 0; c < columns; c++)
        //        {
        //            cells[cells.Count - 1].Add(input.ReadString());
        //        }
        //    }

        //    UpdateHeaders(Settings.EnsureUniqueRowHeaders, Settings.EnsureUniqueColumnHeaders);
        //}
        #endregion
        #endregion // Constructors

        #region Methods
        // -- METHODS -- 

        #region get
        // --- GET --- with parse method
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the passed parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="parseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.Parse</code>)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(int column, int row, Func<string, T> parseMethod)
        {
            return parseMethod(this[column, row]);
        }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the passed parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="parseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.Parse</code>)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(string column, int row, Func<string, T> parseMethod)    { return Get<T>(columnHeaders.IndexOf(column), row, parseMethod); }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the passed parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="parseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.Parse</code>)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(int column, string row, Func<string, T> parseMethod)    { return Get<T>(column, rowHeaders.IndexOf(row), parseMethod); }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the passed parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="parseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.Parse</code>)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(string column, string row, Func<string, T> parseMethod) { return Get<T>(columnHeaders.IndexOf(column), rowHeaders.IndexOf(row), parseMethod); }

        // --- GET --- without parse method
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(int column, int row)
        {
            return Parser.Parse<T>(this[column, row]);
        }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row of the cell</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(string column, int row) { return Get<T>(columnHeaders.IndexOf(column), row); }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(int column, string row) { return Get<T>(column, rowHeaders.IndexOf(row)); }
        /// <summary>
        /// Gets the data content at the given cell and converts it to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <returns>the content of the cell converted to the desired type</returns>
        public T Get<T>(string column, string row) 
        {
            DEBUG.Assert(columnHeaders.Contains(column), "Table '"+FileName+"' does not contain a column '"+column+"'");
            DEBUG.Assert(rowHeaders.Contains(row), "Table '" + FileName + "' does not contain a row '" + row + "'");
            return Get<T>(columnHeaders.IndexOf(column), rowHeaders.IndexOf(row));
        }

        // --- TRY GET --- with parse method
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the passed try-parse-method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <param name="tryParseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.TryParse</code>)</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(int column, int row, out T result, TryParse<T> tryParseMethod)
        {
            if (!Contains(column, row))
            {
                result = default(T);
                return false;
            }
            return tryParseMethod(this[column, row], out result);
        }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the passed try-parse-method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <param name="tryParseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.TryParse</code>)</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(string column, int row, out T result, TryParse<T> tryParseMethod) { return TryGet<T>(columnHeaders.IndexOf(column), row, out result, tryParseMethod); }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the passed try-parse-method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <param name="tryParseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.TryParse</code>)</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(int column, string row, out T result, TryParse<T> tryParseMethod) { return TryGet<T>(column, rowHeaders.IndexOf(row), out result, tryParseMethod); }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the passed try-parse-method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <param name="tryParseMethod">the method which is used to parse the content of the cell to the desired type (in common <code>MyType.TryParse</code>)</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(string column, string row, out T result, TryParse<T> tryParseMethod) { return TryGet<T>(columnHeaders.IndexOf(column), rowHeaders.IndexOf(row), out result, tryParseMethod); }

        // --- TRY GET --- without parse method
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(int column, int row, out T result)
        {
            if (!Contains(column, row))
            {
                result = default(T);
                return false;
            }

            return Parser.TryParse<T>(this[column, row], out result);
        }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(string column, int row, out T result) { return TryGet<T>(columnHeaders.IndexOf(column), row, out result); }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(int column, string row, out T result) { return TryGet<T>(column, rowHeaders.IndexOf(row), out result); }
        /// <summary>
        /// Tries to get the data content at the given cell converted to the given type by using the registered type related parse method
        /// </summary>
        /// <typeparam name="T">the type to retrieve</typeparam>
        /// <param name="column">the column-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="row">the row-header of the cell (if there are more than one with the same name, the first occurance will be returned)</param>
        /// <param name="result">the content of the cell converted to the desired type</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        public bool TryGet<T>(string column, string row, out T result) { return TryGet<T>(columnHeaders.IndexOf(column), rowHeaders.IndexOf(row), out result); }

        /// <summary>
        /// Tries to get the data content at the given cell by using the registered type related parse method
        /// </summary>
        /// <typeparam name="type">the type to retrieve</typeparam>
        /// <param name="column">the column of the cell</param>
        /// <param name="row">the row of the cell</param>
        /// <param name="result">the content of the cell as the desired type</param>
        /// <returns>true, if the parse process was successfull. Otherwise false.</returns>
        /// <remarks>this method is used for ComponentHelper.OverrideComponentProperties. In the very most cases it is better to use one of the generic methods.</remarks>
        public object Get(Type type, int column, int row)
        {
            return Parser.Parse(type, this[column, row]);
        }
        #endregion


        #region iterators

        /// <summary>
        /// returns every cell as a string of the given row (without the header)
        /// </summary>
        /// <param name="row">the row-header for the row to get</param>
        /// <returns>an iterator through all cells (as strings) in the row</returns>
        public IEnumerable<string> GetRowCellIterator(string row)                       { return GetRowCellIterator(rowHeaders.IndexOf(row), false); }
        /// <summary>
        /// returns every cell as a string of the given row
        /// </summary>
        /// <param name="row">the row-header for the row to get</param>
        /// <param name="includeHeader">if true, the header row is included. otherwise it is without the header</param>
        /// <returns>an iterator through all cells (as strings) in the row</returns>
        public IEnumerable<string> GetRowCellIterator(string row, bool includeHeader)   { return GetRowCellIterator(rowHeaders.IndexOf(row), includeHeader); }
        /// <summary>
        /// returns every cell as a string of the given row (without the header)
        /// </summary>
        /// <param name="row">the row number to get</param>
        /// <returns>an iterator through all cells (as strings) in the row</returns>
        public IEnumerable<string> GetRowCellIterator(int row)                          { return GetRowCellIterator(row, false); }
        /// <summary>
        /// returns every cell as a string of the given row
        /// </summary>
        /// <param name="row">the row number to get</param>
        /// <param name="includeHeader">if true, the header row is included. otherwise it is without the header</param>
        /// <returns>an iterator through all cells (as strings) in the row</returns>
        public IEnumerable<string> GetRowCellIterator(int row, bool includeHeader)
        {
            for (int i = 0; i < Columns; i++)
            {
                if (includeHeader || i != Settings.RowHeaderIndex)
                    yield return cells[row][i];
            }
        }
        /// <summary>
        /// returns every cell as a string of the given column (without the header)
        /// </summary>
        /// <param name="column">the column-header for the column to get</param>
        /// <returns>an iterator through all cells (as strings) in the column</returns>
        public IEnumerable<string> GetColumnCellIterator(string column)                     { return GetColumnCellIterator(columnHeaders.IndexOf(column), false); }
        /// <summary>
        /// returns every cell as a string of the given column
        /// </summary>
        /// <param name="column">the column-header for the column to get</param>
        /// <param name="includeHeader">if true, the header column is included. otherwise it is without the header</param>
        /// <returns>an iterator through all cells (as strings) in the column</returns>
        public IEnumerable<string> GetColumnCellIterator(string column, bool includeHeader) { return GetColumnCellIterator(columnHeaders.IndexOf(column), includeHeader); }
        /// <summary>
        /// returns every cell as a string of the given column (without the header)
        /// </summary>
        /// <param name="column">the column number to get</param>
        /// <returns>an iterator through all cells (as strings) in the column</returns>
        public IEnumerable<string> GetColumnCellIterator(int column)                        { return GetColumnCellIterator(column, false); }
        /// <summary>
        /// returns every cell as a string of the given column
        /// </summary>
        /// <param name="column">the column number to get</param>
        /// <param name="includeHeader">if true, the header column is included. otherwise it is without the header</param>
        /// <returns>an iterator through all cells (as strings) in the column</returns>
        public IEnumerable<string> GetColumnCellIterator(int column, bool includeHeader)
        {
            for (int i = 0; i < Rows; i++)
            {
                if(includeHeader || i != Settings.ColumnHeaderIndex)
                    yield return cells[i][column];
            }
        }
		
		/// <summary>
		/// Small wrapper around one line to easily access cells from a line via column header
		/// </summary>
		public class Line 
		{
			private int row;
			private Table table;
			
			public Line(Table table, int row)
			{
				this.row = row;
				this.table = table;
			}
			
			public int Row
			{
				get { return row; }
			}
			
			public string this[int i]
			{
				get { try { return table[i, row]; } catch { return null; } }
				set { table[i, row] = value; }
			}
			
			public string this[string key]
			{
				get { try { return table[table.GetColumnIndex(key), row]; } catch { return null; } }
				set { table[table.GetColumnIndex(key), row] = value; }
			}
		}
		
		/// <summary>
        /// Iterates through all lines and return a whole line as access object.
        /// </summary>
        public IEnumerable<Line> GetExtendedLineIterator()
        {
			for(int row = 0; row < Rows; ++row)
                yield return new Line(this, row);
        }
		
        /// <summary>
        /// Iterates through all lines and return a whole line as string array.
        /// </summary>
        public IEnumerable<List<string>> GetLineIterator()
        {
            foreach (var line in cells)
                yield return line;
        }

        /// <summary>
        /// Iterates through all cells of the Table except the header cells
        /// </summary>
        /// <returns>the cell iterator</returns>
        public IEnumerable<string> GetTableIterator()                                       { return GetTableIterator(false); }
        /// <summary>
        /// Iterates through all cells of the Table
        /// </summary>
        /// <param name="includeHeaders">if true, the header-cells will be returned as well</param>
        /// <returns>the cell iterator</returns>
        public IEnumerable<string> GetTableIterator(bool includeHeaders)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (!includeHeaders && r == Settings.RowHeaderIndex)
                    continue;
                for (int c = 0; c < Columns; c++)
                {
                    if (!includeHeaders && c == Settings.RowHeaderIndex)
                        continue;


                    yield return cells[r][c];
                }
            }
        }

        #endregion


        #region get index
        /// <summary>
        /// Gets the row index to the corresponding header name (first occurance)
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <returns>the row index</returns>
        public int GetRowIndex(string headerName)
        {
            return rowHeaders.IndexOf(headerName);
        }
        /// <summary>
        /// Gets the row index to the corresponding header name (first occurance)
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="ignoreCase">if true, there will be no case sensitivity</param>
        /// <returns>the row index</returns>
        public int GetRowIndex(string headerName, bool ignoreCase)
        {
            if (!ignoreCase)
                return rowHeaders.IndexOf(headerName);

            for (int i = 0; i < rowHeaders.Count; i++)
                if (rowHeaders[i].ToLower() == headerName.ToLower())
                    return i;

            return -1;
        }
        /// <summary>
        /// Iterates through all Rows with the given name and returns the indices.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <returns>all indices with the respective name</returns>
        public IEnumerable<int> GetRowIndices(string headerName)
        {
            return GetRowIndices(headerName, false);
        }
        /// <summary>
        /// Iterates through all Rows with the given name and returns the indices.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="ignoreCase">if true, there will be no case sensitivity</param>
        /// <returns>all indices with the respective name</returns>
        public IEnumerable<int> GetRowIndices(string headerName, bool ignoreCase)
        {
            for (int i = 0; i < rowHeaders.Count; i++)
                if ((ignoreCase && rowHeaders[i].ToLower() == headerName.ToLower())
                    || (!ignoreCase && rowHeaders[i] == headerName))
                    yield return i;
        }

        /// <summary>
        /// Gets the column index to the corresponding header name (first occurance)
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <returns>the column index</returns>
        public int GetColumnIndex(string headerName)
        {
            return columnHeaders.IndexOf(headerName);
        }
        /// <summary>
        /// Gets the row index to the corresponding header name (first occurance)
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="ignoreCase">if true, there will be no case sensitivity</param>
        /// <returns>the row index</returns>
        public int GetColumnIndex(string headerName, bool ignoreCase)
        {
            if (!ignoreCase)
                return columnHeaders.IndexOf(headerName);

            for (int i = 0; i < columnHeaders.Count; i++)
                if (columnHeaders[i].ToLower() == headerName.ToLower())
                    return i;

            return -1;
        }
        /// <summary>
        /// Iterates through all Columns with the given name and returns the indices.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <returns>all indices with the respective name</returns>
        public IEnumerable<int> GetColumnIndices(string headerName)
        {
            return GetColumnIndices(headerName, false);
        }
        /// <summary>
        /// Iterates through all Columns with the given name and returns the indices.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="ignoreCase">if true, there will be no case sensitivity</param>
        /// <returns>all indices with the respective name</returns>
        public IEnumerable<int> GetColumnIndices(string headerName, bool ignoreCase)
        {
            for (int i = 0; i < columnHeaders.Count; i++)
                if ((ignoreCase && columnHeaders[i].ToLower() == headerName.ToLower())
                    || (!ignoreCase && columnHeaders[i] == headerName))
                    yield return i;
        }


        /// <summary>
        /// Iterates through all entries in the given column and returns their indices as long as the corresponing cell is not empty
        /// </summary>
        /// <param name="columnName">the name of the column to iterate through</param>
        /// <returns>all indices which have non-empty cells</returns>
        public IEnumerable<int> GetFilledRowIterator(string columnName)
        {
            return GetFilledRowIterator(GetColumnIndex(columnName), false);
        }
        /// <summary>
        /// Iterates through all entries in the given column and returns their indices as long as the corresponing cell is not empty
        /// </summary>
        /// <param name="columnIndex">the index of the column to iterate through</param>
        /// <param name="includeHeader">if true, also the index of the header will be returned. otherwise not</param>
        /// <returns>all indices which have non-empty cells</returns>
        public IEnumerable<int> GetFilledRowIterator(int columnIndex, bool includeHeader)
        {
            for (int i = 0; i < Rows; i++)
            {
                if (string.IsNullOrEmpty(this[columnIndex, i]) || (!includeHeader && Settings.RowHeaderIndex == i))
                    continue;

                yield return i;
            }
        }
        /// <summary>
        /// Iterates through all entries in the given row and returns their indices as long as the corresponing cell is not empty
        /// </summary>
        /// <param name="rowName">the name of the row to iterate through</param>
        /// <returns>all indices which have non-empty cells</returns>
        public IEnumerable<int> GetFilledColumnIterator(string rowName)
        {
            return GetFilledColumnIterator(GetRowIndex(rowName), false);
        }
        /// <summary>
        /// Iterates through all entries in the given row and returns their indices as long as the corresponing cell is not empty
        /// </summary>
        /// <param name="rowIndex">the index of the row to iterate through</param>
        /// <param name="includeHeader">if true, also the index of the header will be returned. otherwise not</param>
        /// <returns>all indices which have non-empty cells</returns>
        public IEnumerable<int> GetFilledColumnIterator(int rowIndex, bool includeHeader)
        {
            for (int i = 0; i < Columns; i++)
            {
                if (string.IsNullOrEmpty(this[rowIndex, i]) || (includeHeader && Settings.ColumnHeaderIndex == i))
                    continue;

                yield return i;
            }
        }
        #endregion


        #region contains
        /// <summary>
        /// Checks wether the column- and row-headers exists
        /// </summary>
        /// <param name="column">the column header</param>
        /// <param name="row">the row header</param>
        /// <returns>true, if both exist</returns>
        public bool Contains(string column, string row)
        {
            return ContainsColumn(column) && ContainsRow(row);
        }
        /// <summary>
        /// Checks wether the column-index is valid and the row-header exists
        /// </summary>
        /// <param name="column">the column index</param>
        /// <param name="row">the row header</param>
        /// <returns>true, if both are valid</returns>
        public bool Contains(int column, string row)
        {
            return ContainsColumn(column) && ContainsRow(row);
        }
        /// <summary>
        /// Checks wether the column-header exists and the row-index is valid
        /// </summary>
        /// <param name="column">the column header</param>
        /// <param name="row">the row index</param>
        /// <returns>true, if both are valid</returns>
        public bool Contains(string column, int row)
        {
            return ContainsColumn(column) && ContainsRow(row);
        }
        /// <summary>
        /// Checks wether the passed column- and row-indices are valid
        /// </summary>
        /// <param name="column">the column index</param>
        /// <param name="row">the row index</param>
        /// <returns>true, if both are valid</returns>
        public bool Contains(int column, int row)
        {
            return ContainsColumn(column) && ContainsRow(row);
        }
        /// <summary>
        /// Checks wether the given row-header exists in the stored headers
        /// </summary>
        /// <param name="header">the name of the header to check</param>
        /// <returns>true, if the header exists</returns>
        public bool ContainsRow(string header)
        {
            return rowHeaders.Contains(header);
        }
        /// <summary>
        /// Checks wether the given row-index is valid
        /// </summary>
        /// <param name="index">the index to check</param>
        /// <returns>true, if valid</returns>
        public bool ContainsRow(int index)
        {
            return (index >= 0) && (index < Rows) && (Rows > 0);
        }
        /// <summary>
        /// Checks wether the given column-header exists in the stored headers
        /// </summary>
        /// <param name="header">the name of the header to check</param>
        /// <returns>true, if the header exists</returns>
        public bool ContainsColumn(string header)
        {
            return columnHeaders.Contains(header);
        }  
        /// <summary>
        /// Checks wether the given column-index is valid
        /// </summary>
        /// <param name="index">the index to check</param>
        /// <returns>true, if valid</returns>
        public bool ContainsColumn(int index)
        {
            return (index >= 0) && (index < Columns) && (Columns > 0);
        }

        #endregion


        #region Load

        //public static bool TryLoadTable(string filename, CSVSetting settings, bool isolatedStorageFile, out Table result)
        //{
        //    result = null;
        //    return false;
        //}

        private void LoadFile(string fileName, CSVSetting settings, bool isolatedStorageFile)
        {
            //using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
#if XNA
            if (isolatedStorageFile)
            {
                using (IsolatedStorageFile file = Helper.GetUserStoreForAppDomain())
                {

                    using (IsolatedStorageFileStream stream = file.OpenFile(fileName, FileMode.Open))
                    {
                        LoadFromStream(stream, settings);

                        stream.Close();
                    }
                }
            }
            else
#endif
            {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
                //TODO KORION IO
#else
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    LoadFromStream(stream, settings);

                    stream.Close();
                }
#endif
            }

            if (cells[cells.Count - 1].Count == 0)
                cells.RemoveAt(cells.Count - 1);
        }

        private void LoadFromStream(Stream stream, CSVSetting settings)
        {
            string input;
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                input = sr.ReadToEnd();
                sr.Close();
            }
            LoadFromString(input, settings);
        }
        private void LoadFromString(string input, CSVSetting settings)
        {
            cells.Add(new List<string>());

            string field = "";
            bool isInsideString = false;
            bool isInsideStringCell = false;
            int maxWidth = 0;
            int columnCounter = 0;

            bool isCurrentRowCommented = false;
            List<int> commentedColumns = new List<int>();
			
            char prev = '\0';

            for (int i = 0; i < input.Length - 1; i++)
            {
                if (i > 0)
                    prev = input[i - 1];

                char c = input[i];
                char next = input[i + 1];

                if ((i == 0 && c == 0xEF) || (i == 1 && c == 0xBB) || (i == 2 && c == 0xBF)) // UTF-BOM
                    continue;

                {
                    if (!isInsideString && !isInsideStringCell) // inside string
                    {
                        if ((c == settings.ColumnSeparator)) // Switch Column to right
                        {
                            if (!isCurrentRowCommented && !commentedColumns.Contains(columnCounter))
                                cells[cells.Count - 1].Add(field);

                            columnCounter++;
                            field = "";
                            continue;
                        }
                        else if (c == '\n') // Switch Row down
                        {
                            if (!isCurrentRowCommented)
                            {
                                cells[cells.Count - 1].Add(field);
                                if (maxWidth < cells[cells.Count - 1].Count)
                                {
                                    maxWidth = cells[cells.Count - 1].Count;
                                }

                                cells.Add(new List<string>());
                            }
                            isCurrentRowCommented = false;
                            columnCounter = 0;
                            field = "";
                            continue;
                        }
                        else if (c == settings.StringIdentifier) // Check for String identifier
                        {
                            if (prev == settings.ColumnSeparator || prev == '\n' || prev == '\r')
                            {
                                isInsideStringCell = true;
                                continue;
                            }
                            else
                            {
                                isInsideString = true;
                            }
                        }
                    }
                    else // inside string
                    {
                        //handle double quotes (escaped quotes)
                        if (c == settings.StringIdentifier && next == settings.StringIdentifier)
                        {
                            field += c;
                            i++;
                            continue;
                        }
                        if (c == settings.StringIdentifier) // Check for String identifier
                        {
                            if(isInsideString)
                            {
                                isInsideString = false;
                            }
                            else if(next == settings.ColumnSeparator || next == '\n' || next == '\r')
                            {
                                isInsideStringCell = false;
                                continue;
                            }
                        }
                            // check: shouldn't this be on the very top of the loop?
                        else if (c == '“' || c == '”') // these symbols might be the replacement for string identifiers.
                            c = '"';                   // but if we parse script-code, it has to be the normal quotes.
                    }

                    if (c != '\r')
                    {
                        field += c;
                    }

                    // ignore commented lines
                    if (field == settings.CommentIdentifier)
                    {
                        if (cells.Count - 1 == settings.ColumnHeaderIndex)
                        {
                            commentedColumns.Add(columnCounter);
                        }
                        else if (cells[cells.Count - 1].Count == settings.RowHeaderIndex)
                        {
                            isCurrentRowCommented = true;
                        }
                    }
                }

                if (i == input.Length - 2)//(c == '\0' || next == '\0')
                {
                    if (field.EndsWith(settings.StringIdentifier.ToString()))
                        field = field.Remove(field.Length - 1);

                    cells[cells.Count - 1].Add(field);
                    if (maxWidth < cells[cells.Count - 1].Count)
                    {
                        maxWidth = cells[cells.Count - 1].Count;
                    }

                    //cells.Add(new List<string>());
                    field = "";
                }
                
            }
        }

#endregion

        #region save
        /// <summary>
        /// saves the table to a csv file. It uses the file name which is specified in the <c>FileName</c> property
        /// </summary>
        public void Save()
        {
            Save(fileName, Settings);
        }
        /// <summary>
        /// saves the table to a csv file.
        /// </summary>
        /// <param name="fileName">the path and file name with extension</param>
        public void Save(string fileName)
        {
            Save(fileName, Settings);
        }

       // #if XNA
        /// <summary>
        /// saves the table to a csv file.
        /// </summary>
        /// <param name="fileName">the path and file name with extension</param>
        /// <param name="setting">the setting which is used for formatting</param>
        public void Save(string fileName, CSVSetting setting)
        {
#if XNA
            using (IsolatedStorageFile file = Helper.GetUserStoreForAppDomain())
            {
                string dir = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(dir) && !file.DirectoryExists(dir))
                    file.CreateDirectory(dir);

                if (file.FileExists(fileName))
                    file.DeleteFile(fileName);

                using (IsolatedStorageFileStream stream = file.OpenFile(fileName, FileMode.OpenOrCreate))
                {
                    Save(stream, setting);
                    stream.Close();
                }
            }
#elif UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
            //TODO KORION IO
#else
            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                Save(stream, setting);

                stream.Close();
            }
#endif
        }

        public void Save(Stream stream, CSVSetting setting)
        {
            var encoding = new UTF8Encoding(false);
            using (StreamWriter sw = new StreamWriter(stream, encoding))
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int k = 0; k < Columns; k++)
                    {
                        string cell = this[k, i];

                        if (setting.UseStringIdentifierOnlyOnNewLine)
                        {
                            if (cell.Contains('\n') || cell.Contains('\r'))
                                sw.Write(string.Format("{0}{1}{0}", setting.StringIdentifier, cell));
                            else
                                sw.Write(cell);
                        }
                        else
                        {
                            sw.Write(string.Format("{0}{1}{0}", setting.StringIdentifier, cell));
                        }

                        if (k != Columns - 1)
                            sw.Write(setting.ColumnSeparator);
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
            }
        }

        #endregion

        #region sort
        /// <summary>
        /// Sorts the Table by the values of a given column. 
        /// cells which can not be converted to the given type will stay at the top of the table
        /// </summary>
        /// <typeparam name="T">the type of the cells which are used for sorting</typeparam>
        /// <param name="column">the column where the cells for sorting are</param>
        /// <param name="ascending">if true, the table is sorted from low to high, if false reversed.</param>
        public void SortByColumn<T>(string column, bool ascending)
            where T : IComparable<T>
        {
            SortByColumn<T>(columnHeaders.IndexOf(column), ascending);
        }
        /// <summary>
        /// Sorts the Table by the values of a given column. 
        /// cells which can not be converted to the given type will stay at the top of the table
        /// </summary>
        /// <typeparam name="T">the type of the cells which are used for sorting</typeparam>
        /// <param name="column">the column where the cells for sorting are</param>
        /// <param name="ascending">if true, the table is sorted from low to high, if false reversed.</param>
        public void SortByColumn<T>(int column, bool ascending)
            where T : IComparable<T>
        {
            List<KeyValuePair<T, int>> entries = new List<KeyValuePair<T, int>>();
            int idx = 0;
            foreach (string entry in GetColumnCellIterator(column, true))
            {
                T result;
                if (Parser.TryParse<T>(entry, out result))
                {
                    entries.Add(new KeyValuePair<T, int>(result, idx));
                }
                idx++;
            }
            entries.Sort(Extensions.CompareKeys);
            if (!ascending)
                entries.Reverse();

            // add sorted at the end
            for (int i = 0; i < entries.Count; i++)
                cells.Add(cells[entries[i].Value]);

            // remove unsorted previous entries
            entries.Sort(Extensions.CompareValues);
            for (int i = entries.Count - 1; i >= 0; i--)
                cells.RemoveAt(entries[i].Value);

            UpdateHeaders(false, false);
        }
        
        /// <summary>
        /// Sorts the Table by the strings of a given column. 
        /// </summary>
        /// <param name="column">the column where the cells for sorting are</param>
        /// <param name="ascending">if true, the table is sorted from low to high, if false reversed.</param>
        /// <param name="leaveHeaderOnTop">if true, the upper row will stay on the top after sorting</param>
        public void SortByColumn(string column, bool ascending, bool leaveHeaderOnTop)
        {
            SortByColumn(columnHeaders.IndexOf(column), ascending, leaveHeaderOnTop);
        }
        /// <summary>
        /// Sorts the Table by the strings of a given column. 
        /// </summary>
        /// <param name="column">the column where the cells for sorting are</param>
        /// <param name="ascending">if true, the table is sorted from low to high, if false reversed.</param>
        /// <param name="leaveHeaderOnTop">if true, the upper row will stay on the top after sorting</param>
        public void SortByColumn(int column, bool ascending, bool leaveHeaderOnTop)
        {
            List<string> header = columnHeaders;
            
            if (ascending)
                cells.Sort((a, b) => { return a[column].CompareTo(b[column]); });
            else
                cells.Sort((a, b) => { return b[column].CompareTo(a[column]); });

            if (leaveHeaderOnTop)
            {
                cells.Remove(header);
                cells.Insert(0, header);
            }

            UpdateHeaders(false, false);
        }

        #endregion

        /// <summary>
        /// Mirrors the table. After calling this method, the rows will be columns and the columns will be rows.
        /// </summary>
        public void Mirror()
        {
            List<List<string>> newCells = new List<List<string>>();
            for (int col = 0; col < Columns; col++)
            {
                newCells.Add(new List<string>());
                for (int row = 0; row < Rows; row++)
                {
                    newCells[col].Add(cells[row][col]);
                }
            }

            cells = newCells;
            UpdateHeaders(false, false);
        }

        private void Set(int column, int row, string value)
        {
            // take care of the row-headers. They are an independet list.
            // not so the column-headers (they just point to a list inside cells)
            if (column == Settings.RowHeaderIndex)
                rowHeaders[row] = value;

            cells[row][column] = value;
        }

        private void RemoveEmptyRows()
        {
            for (int r = cells.Count-1; r >= 0; r--)
            {
                bool isEmpty = true;
                for (int c = 0; c < cells[r].Count; c++)
                {
                    string cell = cells[r][c];
                    if (!string.IsNullOrEmpty(cell) && cell != "\n")
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty) 
                    cells.RemoveAt(r);
            }
        }

        public void UpdateHeaders()
        {
            UpdateHeaders(Settings.EnsureUniqueRowHeaders, Settings.EnsureUniqueColumnHeaders);
        }
        private void UpdateHeaders(bool ensureUniqueRowHeaders, bool ensureUniqueColumnHeaders)
        {
            RemoveEmptyRows();
            
            // Fill row and column headers
#if DEBUG
            List<string> tmpColumnHeaders = new List<string>();
            for (int i = 0; i < cells[Settings.RowHeaderIndex].Count; i++)
            {
                DEBUG.Assert(!ensureUniqueColumnHeaders || (ensureUniqueColumnHeaders && !tmpColumnHeaders.Contains(cells[Settings.ColumnHeaderIndex][i])),
                    "the table " + fileName + " needs unique column headers. The header \"" + cells[Settings.ColumnHeaderIndex][i] + "\" is used more often than once.");
                tmpColumnHeaders.Add(cells[Settings.ColumnHeaderIndex][i]);
            }
#endif
            columnHeaders = cells[Settings.ColumnHeaderIndex];


            rowHeaders.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                DEBUG.Assert(!ensureUniqueRowHeaders || (ensureUniqueRowHeaders && !rowHeaders.Contains(cells[i][Settings.RowHeaderIndex])),
                    "the table " + fileName + " needs unique row headers. The header \"" + cells[i][Settings.RowHeaderIndex] + "\" is used more often than once.");
                rowHeaders.Add(cells[i][Settings.RowHeaderIndex]);
            }
        }

        public void AppendRow(params string[] cells)
        {
            IEnumerable<string> list = cells.ToList();
            if (this.cells.Count > 0)
            {
                // make the parameter the same length as our list by either striping or filling with ""
                if (cells.Length > this.cells[0].Count) list = list.Take(cells.Length);
                if (cells.Length < this.cells[0].Count) list = list.Concat(Enumerable.Repeat("", this.cells[0].Count - cells.Length));
            }
            this.cells.Add(list.ToList());
            UpdateHeaders(false, false);
        }


        public void RemoveRow(int rowIndex)
        {
            this.cells.RemoveAt(rowIndex);
            UpdateHeaders(false, false);
        }
        public void RemoveRowRange(int rowIndex, int count)
        {
            if (rowIndex < Rows)
            {
                this.cells.RemoveRange(rowIndex, count);
                UpdateHeaders(false, false);
            }
        }


        public void AppendTable(Table table)
        {
            if (table.cells.Count > 0)
            {
                cells.AddRange(table.GetLineIterator());
                UpdateHeaders(false, false);
            }
        }
#endregion // Methods

        public void Clear(bool clearHeaderColumns)
        {
            cells.Clear();
            
            rowHeaders.Clear();

            if (!clearHeaderColumns)
            {
                AppendRow(columnHeaders.ToArray());
            }
        }
    }

    #region XNA Loading

    //public class TableReader : ContentTypeReader<Table>
    //{
    //    protected override Table Read(ContentReader input, Table existingInstance)
    //    {
    //        if (existingInstance == null)
    //            existingInstance = new Table(input.ReadString(), 0, 0);

    //        existingInstance.FillWithContentData(input);

    //        return existingInstance;
    //    }
    //}
    #endregion

}
