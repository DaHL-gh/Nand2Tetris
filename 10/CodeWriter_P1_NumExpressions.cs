using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>2+x</summary>
        public void WriteExpression(ExpressionSyntax expression)
        {
            WriteTerm(expression.Term);

            foreach (ExpressionTail tail in expression.Tail)
            {
                WriteTerm(tail.Term);
                WriteOperator(tail.Operator);
            }
        }

        private void WriteTerm(TermSyntax term)
        {
            var ok = TryWriteStringValue(term) // будет реализована в следующих задачах
                     || TryWriteArrayAccess(term) // будет реализована в следующих задачах
                     || TryWriteObjectValue(term) // будет реализована в следующих задачах
                     || TryWriteSubroutineCall(term) // будет реализована в следующих задачах
                     || TryWriteNumericTerm(term);
            if (!ok)
                throw new FormatException($"Unknown term [{term}]");
        }

        /// <summary>42 | true | false | varName | -x | ( x )</summary>
        private bool TryWriteNumericTerm(TermSyntax term)
        {
            switch (term)
            {
                case ValueTermSyntax valueTerm:
                    return TryWriteValueTerm(valueTerm);
                case ParenthesizedTermSyntax parenthesizedTerm:
                    WriteExpression(parenthesizedTerm.Expression);
                    return true;
                case UnaryOpTermSyntax unaryOpTerm:
                    WriteTerm(unaryOpTerm.Term);
                    WriteUnaryOperator(unaryOpTerm.UnaryOp);
                    return true;
                default:
                    return false;
            }
        }

        private bool TryWriteValueTerm(ValueTermSyntax valueTerm)
        {
            if (valueTerm.Indexing != null)
                return false;

            string? constant = null;
            if (valueTerm.Value.TokenType == TokenType.IntegerConstant)
                constant = valueTerm.Value.Value;
            else if (valueTerm.Value.Value == "true")
                constant = "-1";
            else if (valueTerm.Value.Value == "false")
                constant = "0";

            if (constant != null) {
                Write("push constant " + constant);
                return true;
            }

            if (valueTerm.Value.TokenType == TokenType.Identifier) {
                VarInfo? varInfo = FindVarInfo(valueTerm.Value.Value);
                if (varInfo == null)
                    Console.WriteLine($"WARNING wasnt found info for {valueTerm.Value.Value}");

                Write($"push {varInfo.SegmentName} {varInfo.Index}");
                return true;
            }
            return false;
        }

        private void WriteOperator(Token op)
        {
            if (op.Value == "+") Write("add");
            else if (op.Value == "-") Write("sub");
            else if (op.Value == "=") Write("eq");
            else if (op.Value == ">") Write("gt");
            else if (op.Value == "<") Write("lt");
            else if (op.Value == "&") Write("and");
            else if (op.Value == "|") Write("or");
            else if (op.Value == "*") Write("call Math.multiply 2");
            else if (op.Value == "/") Write("call Math.divide 2");
            else
                Console.WriteLine("WARNING no action for operator" + op.Value);
        }

        private void WriteUnaryOperator(Token unaryOp)
        {
            if (unaryOp.Value == "-")
                Write("neg");
            else if (unaryOp.Value == "~")
                Write("not");
            else
                Console.WriteLine("WARNING no action for unary operator" + unaryOp.Value);
        }
    }
}
