using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator
{
    public abstract class TextContent : IEquatable<TextContent>
    {
        public abstract ContentType ContentType { get; }

        public abstract bool Equals(TextContent other);
    }
}
