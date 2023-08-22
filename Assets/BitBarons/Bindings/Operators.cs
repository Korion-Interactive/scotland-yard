using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitBarons.Bindings
{
    public enum MathOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
    }
    public static class MathOperatorExtensions
    {
        public static string GetOperatorSignCSharp(this MathOperator self)
        {
            switch (self)
            {
                case MathOperator.Add: return "+";
                case MathOperator.Subtract: return "-";
                case MathOperator.Multiply: return "*";
                case MathOperator.Divide: return "/";
                case MathOperator.Modulo: return "%";
                default: return "?";
            }
        }
    }
    public enum BoolOperator
    {
        Equals,
        NotEquals,
        Bigger,
        BiggerOrEqual,
        Smaller,
        SmallerOrEqual,
    }
    public static class BoolOperatorExtensions
    {
        public static string GetOperatorSignCSharp(this BoolOperator self)
        {
            switch (self)
            {
                case BoolOperator.Equals: return "==";
                case BoolOperator.NotEquals: return "!=";
                case BoolOperator.Smaller: return "<";
                case BoolOperator.SmallerOrEqual: return "<=";
                case BoolOperator.Bigger: return ">";
                case BoolOperator.BiggerOrEqual: return ">=";
                default: return "?";
            }
        }
    }
}
