using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Editor.CodeGenerator
{
    public class Instruction : TextContent
    {
        public override ContentType ContentType { get { return CodeGenerator.ContentType.Instruction; } }
        public string InstructionString { get;set;}

        public Instruction(string instructionString)
            : base()
        {
            this.InstructionString = instructionString;
        }

        public override string ToString()
        {
            return InstructionString;
        }

        public static implicit operator Instruction(string instructionString)
        {
            return new Instruction(instructionString);
        }

        public static implicit operator string(Instruction instruction)
        {
            return instruction.InstructionString;
        }


        public override bool Equals(TextContent other)
        {
            if (!(other is Instruction))
                return false;

            var o = other as Instruction;
            return o.InstructionString == this.InstructionString;
        }
    }
}
