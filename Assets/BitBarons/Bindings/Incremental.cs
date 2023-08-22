using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BitBarons.Bindings
{
    [Serializable]
    public class Incremental : BaseNamePart
    {
        public override string NamePartType { get { return BaseNamePart.TYPE_INCREMENT; } }

        public int StartIndex;
        public int Increment = 1;


        public override string GetNamePart(GameObject prefab, int number)
        {
            return (StartIndex + number * Increment).ToString();
        }


        public override void Initialize(GameObject previewObject)
        {
            // nothing to init
        }
    }
}
