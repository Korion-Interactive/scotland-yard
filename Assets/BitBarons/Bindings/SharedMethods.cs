using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BitBarons.Bindings
{
    public static class SharedMethods
    {
        public static string GetStringValue(List<BaseNamePart> nameParts, string format, GameObject o, int idx = 0)
        {
            string[] input = new string[nameParts.Count];
            for (int i = 0; i < input.Length; i++)
            {
                try
                {
                    var part = nameParts[i];
                    input[i] = part.GetNamePart(o, idx);
                }
                catch (Exception)
                {
                    return "< problem with name part " + i + " >";
                }
            }

            try
            {
                return string.Format(format, input);
            }
            catch (Exception)
            {
                return "< formating failed >";
            }
        }

    }
}
