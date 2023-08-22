using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BitBarons.Bindings
{
    [Serializable]
    public class RegExMatches : VariableToString
    {
        public override string NamePartType { get { return BaseNamePart.TYPE_REGEX; } }
        public string RegularExpression;
        //public bool ReplaceMatch;
        //public string Replacement;



        public override string GetNamePart(GameObject prefab, int number)
        {
            string input = base.GetNamePart(prefab, number).ToString();
            Regex regex = new Regex(RegularExpression);
            Match match;

            if (substringEnabled && (SubstringStart > 0 || SubstringLength > 0))
            {
                if (SubstringLength <= 0)
                    match = regex.Match(input, SubstringStart);
                else
                    match = regex.Match(input, SubstringStart, SubstringLength);
            }
            else
            {
                match = regex.Match(input);
            }

            string result = match.Value;
            return result;
        }

    }
}
