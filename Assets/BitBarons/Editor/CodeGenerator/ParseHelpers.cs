using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator
{
    public static class ParseHelpers
    {

        public static TEnum? ContainsAnyValue<TEnum>(string input, bool enumValuesToLower)
            where TEnum : struct, IComparable, IConvertible, IFormattable // Enum derivation
        {
             foreach (TEnum val in Enum.GetValues(typeof(TEnum)))
             {
                 string stringVal = (enumValuesToLower) ? val.ToString().ToLower() : val.ToString();

                 if (input.Contains(stringVal))
                     return val;
             }

             return null;
        }

    }
}
