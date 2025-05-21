using System.Collections.Generic;

namespace JackCompiling;

public record CompilationError(string Message, int LineNumber);

public class Validator
{
    private readonly List<CompilationError> errors = new();

    public IReadOnlyList<CompilationError> Errors => errors;

    public void ValidateClass(ClassSyntax classSyntax)
    {
        foreach (SubroutineDecSyntax subroutine in classSyntax.SubroutineDec)
        {
            bool allSplitsDoReturn = false;
            foreach (StatementSyntax statement in subroutine.SubroutineBody.Statements.Statements)
            {
                if (StatementAlwaysReturns(statement))
                    allSplitsDoReturn = true;
            }

            if (!allSplitsDoReturn)
            {
                var lineNumber = subroutine.SubroutineBody.Open.LineNumber;
                errors.Add(new CompilationError("Not all execution paths return a value", lineNumber));
            }
        }
    }

    public bool IsIfStatementAlwaysReturns(IfStatementSyntax ifStatement)
    {
        if (ifStatement.ElseClause == null)
        {
            return false;
        }

        // check true route
        bool trueReturns = false;
        foreach (StatementSyntax statement in ifStatement.TrueStatements.Statements)
        {
            if (StatementAlwaysReturns(statement))
                trueReturns = true;
        }
        if (!trueReturns)
            return false;

        // check false route
        bool falseReturns = false;
        foreach (StatementSyntax statement in ifStatement.ElseClause.FalseStatements.Statements)
        {
            if (StatementAlwaysReturns(statement))
                falseReturns = true;
        }
        if (!falseReturns)
            return false;

        return true;
    }

    private bool IsWhileStatementReturns(WhileStatementSyntax whileStatement)
    {
        foreach (StatementSyntax statement in whileStatement.Statements.Statements)
        {
            if (StatementAlwaysReturns(statement))
            {
                return true;
            }
        }
        return false;
    }

    private bool StatementAlwaysReturns(StatementSyntax statement)
    {
        switch (statement)
        {
            case ReturnStatementSyntax:
                return true;
            case IfStatementSyntax ifStatement:
                return IsIfStatementAlwaysReturns(ifStatement);
            case WhileStatementSyntax whileStatement:
                return IsWhileStatementReturns(whileStatement);
            default:
                return false;
        }
    }
}
