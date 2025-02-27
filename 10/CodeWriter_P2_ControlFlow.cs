using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>Statement; Statement; ...</summary>
        public void WriteStatements(StatementsSyntax statements)
        {
            foreach (StatementSyntax statement in statements.Statements)
            {
                WriteStatement(statement);
            }
        }

        private void WriteStatement(StatementSyntax statement)
        {
            var ok = TryWriteVarAssignmentStatement(statement)
                     || TryWriteProgramFlowStatement(statement)
                     || TryWriteDoStatement(statement) // будет реализована в следующий задачах
                     || TryWriteArrayAssignmentStatement(statement)  // будет реализована в следующий задачах
                     || TryWriteReturnStatement(statement);  // будет реализована в следующий задачах
            if (!ok)
                throw new FormatException($"Unknown statement [{statement}]");
        }

        /// <summary>let VarName = Expression;</summary>
        private bool TryWriteVarAssignmentStatement(StatementSyntax statement)
        {
            if (statement is LetStatementSyntax letStatement)
            {
                if (letStatement.Index != null)
                {
                    return false;
                }

								string varName = letStatement.VarName.Value;
                VarInfo? varInfo = FindVarInfo(varName);
                if (varInfo == null)
                {
										Console.WriteLine($"WARNING no info found for var {varName}");
                    return false;
                }

                WriteExpression(letStatement.Value);
                Write($"pop {varInfo.SegmentName} {varInfo.Index}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// if ( Expression ) { Statements } [else { Statements }
        /// while ( Expression ) { Statements }
        /// </summary>
        private bool TryWriteProgramFlowStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case IfStatementSyntax ifStatement:
                    WriteIfStatement(ifStatement);
                    return true;

                case WhileStatementSyntax whileStatement:
                    WriteWhileStatement(whileStatement);
                    return true;
            }
            return false;
        }

        private void WriteIfStatement(IfStatementSyntax ifStatement)
        {
            ElseClause? elseClause = ifStatement.ElseClause;
            WriteExpression(ifStatement.Condition);
            Write("not");


            string firstLabel = GenerateUniqueLabel();
            Write($"if-goto {firstLabel}");
            WriteStatements(ifStatement.TrueStatements);

            string? secondLabel = null;
            if (elseClause != null)
            {
                secondLabel = GenerateUniqueLabel();
                Write($"goto {secondLabel}");
            }

            Write($"label {firstLabel}");
            if (elseClause != null && firstLabel != null)
            {
                WriteStatements(elseClause.FalseStatements);
                Write($"label {secondLabel}");
            }
        }

        private void WriteWhileStatement(WhileStatementSyntax whileStatement)
        {
            string whileLabel = GenerateUniqueLabel();
            string breakLabel = GenerateUniqueLabel();
            Write($"label {whileLabel}");

            WriteExpression(whileStatement.Condition);
            Write("not");
            Write($"if-goto {breakLabel}");

            WriteStatements(whileStatement.Statements);
            Write($"goto {whileLabel}");

            Write($"label {breakLabel}");
        }

        private int labelCounter = 0;
        private string GenerateUniqueLabel()
        {
            return $"LABEL{labelCounter++}";
        }
    }
}
