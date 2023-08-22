using System;

namespace Sunbow.Util.IO
{
    /// <summary>
    /// Formatting information of a CSV file
    /// </summary>
    public struct CSVSetting
    {
        /// <summary>The default column separator (not set by default constructor)</summary>
        public const char DEFAULT_COLUMN_SEPARATOR = ';';
        /// <summary>The default string identifier (not set by default constructor)</summary>
        public const char DEFAULT_STRING_IDENTIFIER = '"';
        /// <summary>The default string for comments</summary>
        public const string DEFAULT_COMMENT_IDENTIFIER = "//";

        /// <summary>the default settings. This setting ensures on loading time that all header strings of rows and all header strings of columns are unique</summary>
        public static CSVSetting UniqueHeaders { get { return new CSVSetting(true, true); } }
        /// <summary>This setting ensures on loading time that all header strings of rows are unique</summary>
        public static CSVSetting UniqueRowHeaders { get { return new CSVSetting(false, true); } }
        /// <summary>This setting ensures on loading time that all header strings of columns are unique</summary>
        public static CSVSetting UniqueColumnHeaders { get { return new CSVSetting(true, false); } }
        /// <summary>This setting is for tables where you only want to access the cells by their indices</summary>
        public static CSVSetting RawTableSettings { get { return new CSVSetting(false, false); } }


        /// <summary>the character to separate columns</summary>
        public char ColumnSeparator
        {
            get { return columnSeparator; }
            set
            {
                if(value == '\n' || value == '\r' || value == '\0' || value == '.' || value == '-' || value == stringIdentifier)
                    throw new ArgumentException("illegal column separator: " + value);
                columnSeparator = value;
            }
        }
        char columnSeparator;

        /// <summary>the character to identify strings</summary>
        public char StringIdentifier
        {
            get { return stringIdentifier; }
            set
            {
                if(value == '\n' || value == '\r' || value == '\0' || value == '.' || value == '-' || value == columnSeparator )
                    throw new ArgumentException("illegal string identifier: " + value);
                stringIdentifier = value;
            }
        }
        char stringIdentifier;

        /// <summary>if this character is the first character in a header-row or header-column, the row or column is going to be ignored.</summary>
        public string CommentIdentifier
        {
            get { return commentIdentifier; }
            set
            {
                if(value == "\n" || value == "\r" || value == "\0" || value == "-" || value == columnSeparator.ToString() || value == stringIdentifier.ToString())
                    throw new ArgumentException("illegal comment identifier: " + value);
                commentIdentifier = value;
            }
        }
        string commentIdentifier;

        /// <summary>the index which indicates which row is going to be used for the headers</summary>
        public int ColumnHeaderIndex;
        /// <summary>if true, the table checks wether there are only unique strings used for Column Headers (debug only)</summary>
        public bool EnsureUniqueColumnHeaders;

        /// <summary>the index which indicates which column is going to be used for the headers</summary>
        public int RowHeaderIndex;
        /// <summary>if true, the table checks wether there are only unique strings used for Row Headers (debug only)</summary>
        public bool EnsureUniqueRowHeaders;

        /// <summary>if true, the identifier is only placed if there is one or more new-line characters in the current cell (ignored on load).</summary>
        public bool UseStringIdentifierOnlyOnNewLine;


        /// <summary>
        /// creates a new Setting object
        /// </summary>
        public CSVSetting(bool ensureUniqueColumnHeaders, bool ensureUniqueRowHeaders)
        {
            columnSeparator = DEFAULT_COLUMN_SEPARATOR;
            stringIdentifier = DEFAULT_STRING_IDENTIFIER;

            commentIdentifier = DEFAULT_COMMENT_IDENTIFIER;

            EnsureUniqueColumnHeaders = ensureUniqueColumnHeaders;
            EnsureUniqueRowHeaders = ensureUniqueRowHeaders;

            ColumnHeaderIndex = 0;
            RowHeaderIndex = 0;

            UseStringIdentifierOnlyOnNewLine = true;

        }

        public void SetIdentChars(char colSeparator, char strIdentifier)
        {
            if (colSeparator == '\n' || colSeparator == '\r' || colSeparator == '.' || colSeparator == '-')
                throw new ArgumentException("illegal string identifier: " + colSeparator);
            if (strIdentifier == '\n' || strIdentifier == '\r' || strIdentifier == '.' || strIdentifier == '-')
                throw new ArgumentException("illegal string identifier: " + strIdentifier);
            //if (colSeparator == strIdentifier)
            //    throw new ArgumentException(string.Format("column sparator and string identifier must not be the same! - {0}, {1}",  strIdentifier.ToString(), colSeparator.ToString()));

            this.columnSeparator = colSeparator;
            this.stringIdentifier = strIdentifier;
        }
    }

}