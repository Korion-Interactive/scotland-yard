using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator.Blocks
{
    public class NameSpaceBlock : CSharpBlock
    {
        string nameSpace;

        public NameSpaceBlock(string nameSpace)
            : base(BlockType.Namespace)
        {
            this.nameSpace = nameSpace;
        }

        public override string GetCompleteHeader()
        {
            return "namespace " + nameSpace;
        }


        public static bool TryParseHeader(string input, out CSharpBlock blockHeader)
        {
            if (input.StartsWith("namespace "))
            {
                blockHeader = new NameSpaceBlock(input.Substring("namespace ".Length));
                return true;
            }
            else
            {
                blockHeader = null;
                return false;
            }
        }
    }
}
