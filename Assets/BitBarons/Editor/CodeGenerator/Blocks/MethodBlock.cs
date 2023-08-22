using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitBarons.Util;

namespace BitBarons.Editor.CodeGenerator.Blocks
{
    public class MethodBlock : CSharpBlock
    {
        public string Name { get; private set; }
        public Visibility Visibility { get; set; }
        public bool IsStatic { get; set; }
        public string ReturnType { get; set; }

        public MethodParameter[] Parameters { get; private set; }

        public MethodBlock(string name, Type returnType, params MethodParameter[] parameters)
            : this(name, returnType.GetShortcutName(), parameters)
        { }
        public MethodBlock(string name, string returnType, params MethodParameter[] parameters)
            : base(BlockType.Method)
        {
            this.Name = name;
            this.ReturnType = returnType;
            this.Parameters = parameters;

            this.IsStatic = false;
            this.Visibility = CodeGenerator.Visibility.Public;
        }

        public override string GetCompleteHeader()
        {
            string p = "";
            if(Parameters != null && Parameters.Length > 0)
            {
                foreach (var item in Parameters)
                    p += item.GetParameterString() + ", ";

                p = p.TrimEnd(' ', ',');
            }

            if(IsStatic)
                return string.Format("{0} static {1} {2}({3})", Visibility.ToString().ToLower(), ReturnType, Name, p);
            else
                return string.Format("{0} {1} {2}({3})", Visibility.ToString().ToLower(), ReturnType, Name, p);
        }

        public static bool TryParseHeader(string input, out CSharpBlock blockHeader)
        {
            var parts = input.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length != 2)
            {
                blockHeader = null;
                return false;
            }

            var preParts = parts[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string name = preParts[preParts.Length - 1];
            string retT = preParts[preParts.Length - 2];

            string[] par = parts[1].Split(new char[] { ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
            List<MethodParameter> parameters = new List<MethodParameter>();
            foreach(var p in par)
            {
                MethodParameter output;
                if (MethodParameter.TryParse(p, out output))
                    parameters.Add(output);
            }

            MethodBlock block = new MethodBlock(name, retT, parameters.ToArray());
            block.Visibility = (ParseHelpers.ContainsAnyValue<Visibility>(parts[0], true)) ?? Visibility.Private;
            block.IsStatic = parts[0].Contains("static ");

            blockHeader = block;
            return true;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder(GetCompleteHeader());
            result.AppendLine("{");
            foreach (var instr in Content)
                result.AppendLine("  " + instr.ToString());
            result.AppendLine("}");
            return result.ToString();
        }
    }
}
