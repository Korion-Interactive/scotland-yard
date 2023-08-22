using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator
{
    public enum ContentType
    {
        Unknown,
        Block,
        Instruction,
       // Comment, <- not available yet
    }

    public enum BlockType
    {
        Undefined,

        Namespace,
        Type,
        Method,

        // TODO: other block types
    }

    public enum DataType
    {
        Unknown,
        Class,
        Struct,
        Enum,
    }

    public enum Visibility
    {
        Public,
        Internal,
        Protected,
        Private,
    }


    public enum ParameterKeyword
    {
        None,
        Ref,
        Out,
        Params,
    }
}
