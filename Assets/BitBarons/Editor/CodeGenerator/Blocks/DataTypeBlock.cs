using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator.Blocks
{
    public class DataTypeBlock : CSharpBlock
    {
        public string Name { get; private set; }
        public DataType DataType { get; private set; }

        public string[] Extendings { get; private set; }
        public Visibility Visibility { get; set; }
        public bool IsStatic { get; set; }

        public DataTypeBlock(string name, DataType dataType, params string[] extendings)
            : base(BlockType.Type)
        {
            this.Name = name;
            this.DataType = dataType;
            this.Extendings = extendings;
            this.Visibility = Visibility.Public;
            this.IsStatic = false;
        }
        public override string GetCompleteHeader()
        {
            string ext = "";
            if(Extendings != null && Extendings.Length > 0)
            {
                ext += ": ";

                foreach (var item in Extendings)
                    ext += item + ", ";

                ext = ext.TrimEnd(' ', ',');
            }
            if(IsStatic)
                return string.Format("{0} static {1} {2} {3}", Visibility.ToString().ToLower(), DataType.ToString().ToLower(), Name, ext);
            else
                return string.Format("{0} {1} {2} {3}", Visibility.ToString().ToLower(), DataType.ToString().ToLower(), Name, ext);
        }

        public static bool TryParseHeader(string input, out CSharpBlock blockHeader)
        {
            DataType? dt = ParseHelpers.ContainsAnyValue<DataType>(input, true);//CodeGenerator.DataType.Unknown;
            if(!dt.HasValue)
            {
                blockHeader = null;
                return false;
            }

            DataTypeBlock block;
            if(input.Contains(":"))
            {
                var split = input.Split(':');
                var extendings = split.Last().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var name = split.First().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();

                block = new DataTypeBlock(name, dt.Value, extendings);
            }
            else
            {
                var name = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                block = new DataTypeBlock(name, dt.Value);
            }

            block.Visibility = ParseHelpers.ContainsAnyValue<Visibility>(input, true) ?? Visibility.Private;
            block.IsStatic = input.Contains("static ");

            blockHeader = block;
            return true;
        }

    }
}
