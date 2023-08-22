using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BitBarons.Bindings
{
    [Serializable]
    public abstract class BaseNamePart : ScriptableObject
    {

        public const string TYPE_INCREMENT = "Incremental";
        public const string TYPE_VARIABLE = "Var-2-String";
        public const string TYPE_REGEX = "RegEx Matches";
        public const string TYPE_COMPLEX = "Complex Binding";

        public abstract string NamePartType { get; }

        public abstract string GetNamePart(GameObject prefab, int number);

        public abstract void Initialize(GameObject previewObject);
    }
}
