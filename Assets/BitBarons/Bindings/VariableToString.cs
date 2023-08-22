using BitBarons.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BitBarons.Bindings
{
    [Serializable]
    public class VariableToString : BaseNamePart
    {
        public override string NamePartType { get { return BaseNamePart.TYPE_VARIABLE; } }

        public GameObject PreviewObject { get { return Accessor.Object; } set { Accessor.Object = value; } }
        //public string Component, Variable;
        public VariableAccessor Accessor;

        public bool substringEnabled = false;
        public int SubstringStart = 0;
        public int SubstringLength = 1;
        
        public override void Initialize(GameObject previewObject)
        {
            Accessor = new VariableAccessor() { AccessType = VariableAccessType.Get, };

            if (Accessor.Object == null)
                Accessor.Object = previewObject;
        }


        public override string GetNamePart(GameObject prefab, int number)
        {
            if(prefab != null)
                Accessor.Object = prefab;

            try
            {
                string result = Accessor.GetValue().ToString();

                if(substringEnabled)
                {
                    return result.Substring(SubstringStart, SubstringLength);
                }

                return result;
            }
            catch (Exception)
            {
                return "< ? >";
            }
        }
    }
}
