using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator.Blocks
{

    public class CSharpBlock : TextContent
    {
        public override ContentType ContentType { get { return CodeGenerator.ContentType.Block; } }
        public CSharpCodeCollection Content { get; private set; }
        public BlockType BlockType { get; protected set; }

    
        
        public CSharpBlock()
            : this(BlockType.Undefined)
        {
        }
        protected CSharpBlock(BlockType blockType)
        {
            BlockType = blockType;
            Content = new CSharpCodeCollection();
        }

        public virtual string GetCompleteHeader() { return ""; }

        public override bool Equals(TextContent other)
        {
            if (!(other is CSharpBlock))
                return false;

            var o = other as CSharpBlock;
            return o.GetCompleteHeader() == this.GetCompleteHeader();
        }
    }
}
