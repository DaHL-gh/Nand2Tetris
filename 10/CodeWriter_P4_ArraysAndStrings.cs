using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// "string constant"
        /// </summary>
        private bool TryWriteStringValue(TermSyntax term)
        {
            switch (term)
            {
                case ValueTermSyntax valueTerm:
                    if (valueTerm.Value.TokenType == TokenType.StringConstant)
                    {
                        string value = valueTerm.Value.Value;
                        Write($"push constant {value.Length}");
                        Write("call String.new 1");

                        foreach (char ch in value)
                        {
                            Write($"push constant {Convert.ToInt16(ch)}");
                            Write("call String.appendChar 2");
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// arr[index]
        /// </summary>
        private bool TryWriteArrayAccess(TermSyntax term)
        {
            switch (term)
            {
                case ValueTermSyntax valueTerm:
                    if (valueTerm.Indexing == null)
                        break;

                    VarInfo? varInfo = FindVarInfo(valueTerm.Value.Value);
                    if (varInfo == null)
                        Console.WriteLine($"WARNING wasnt found info for {valueTerm.Value.Value}");

                    Write($"push {varInfo.SegmentName} {varInfo.Index}");
                    WriteExpression(valueTerm.Indexing.Index);
                    Write("add");
                    Write("pop pointer 1");
                    Write("push that 0");

                    return true;
            }
            return false;
        }

        /// <summary>
        /// let arr[index] = expr;
        /// </summary>
        private bool TryWriteArrayAssignmentStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case LetStatementSyntax letStatement:
                    {
                        if (letStatement.Index == null)
                        {
                            break;
                        }
                        VarInfo? varInfo = FindVarInfo(letStatement.VarName.Value);
                        if (varInfo == null)
                            Console.WriteLine($"WARNING wasnt found info for {letStatement.VarName.Value}");

                        Write($"push {varInfo.SegmentName} {varInfo.Index}");
                        WriteExpression(letStatement.Index.Index);
                        Write("add");
                        WriteExpression(letStatement.Value);
                        Write("pop temp 0");

                        Write("pop pointer 1");
                        Write("push temp 0");
                        Write("pop that 0");

                        return true;
                    }
            }
            return false;
        }
    }
}
