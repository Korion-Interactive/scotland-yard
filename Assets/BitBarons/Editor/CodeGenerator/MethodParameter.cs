using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitBarons.Util;

namespace BitBarons.Editor.CodeGenerator
{
    public class MethodParameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public ParameterKeyword Keyword { get; set; }

        public MethodParameter(Type type, string name)
            : this(type.GetShortcutName(), name)
        { }
        public MethodParameter(string type, string name)
        {
            this.Type = type;
            this.Name = name;
            Keyword = ParameterKeyword.None;
        }

        public string GetParameterString()
        {
            string kw = "";
            if (Keyword != ParameterKeyword.None)
                kw = Keyword.ToString().ToLower() + " ";

            return string.Format("{0}{1} {2}", kw, Type, Name);
        }

        public static bool TryParse(string input, out MethodParameter parameter)
        {
            var parts = input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length != 2)
            {
                parameter = null;
                return false;
            }

            parameter = new MethodParameter(parts[0], parts[1]);
            return true;
        }
    }
}
