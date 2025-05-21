using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public class Parser
    {
        private readonly Tokenizer tokenizer;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        private HashSet<string> classVarKeywords = new HashSet<string> { "static", "field" };
        private HashSet<string> subroutineDecKeyword = new HashSet<string> { "constructor", "function", "method" };
        private HashSet<string> statementKeywords = new HashSet<string> { "let", "if", "while", "do", "return" };
        private HashSet<string> opSet = new HashSet<string> { "+", "-", "*", "/", "&", "|", "<", ">", "=" };
        private HashSet<string> unaryOpSet = new HashSet<string> { "-", "~" };
        private HashSet<string> typesSet = new HashSet<string> { "int", "char", "boolean" };
        private HashSet<string> keywordConstant = new HashSet<string> { "true", "false", "null", "this" };

        private bool isTypeToken(Token token)
        {
            return token.TokenType == TokenType.Identifier || typesSet.Contains(token.Value);
        }

        public ClassSyntax ReadClass()
        {
            Token classToken = tokenizer.Read("class");
            Token nameToken = tokenizer.Read(TokenType.Identifier);
            Token openToken = tokenizer.Read("{");

            List<ClassVarDecSyntax> classVars = new List<ClassVarDecSyntax>();
            List<SubroutineDecSyntax> subroutineDecs = new List<SubroutineDecSyntax>();

            Token closeToken = ReadClassMembers(classVars, subroutineDecs);

            return new ClassSyntax(
                Class: classToken,
                Name: nameToken,
                Open: openToken,
                ClassVars: classVars,
                SubroutineDec: subroutineDecs,
                Close: closeToken);
        }

        private Token ReadClassMembers(List<ClassVarDecSyntax> classVars, List<SubroutineDecSyntax> subroutineDecs)
        {
            while (true)
            {
                Token token = tokenizer.Read();

                if (token.Value == "}")
                {
                    return token;
                }

                if (classVarKeywords.Contains(token.Value))
                {
                    tokenizer.PushBack(token);
                    classVars.Add(ReadClassVarDec());
                }
                else if (subroutineDecKeyword.Contains(token.Value))
                {
                    tokenizer.PushBack(token);
                    subroutineDecs.Add(ReadSubroutineDecSyntax());
                }
                else
                {
                    throw new ExpectedException("Expected class variable declaration or subroutine declaration", token);
                }
            }
        }

        private ClassVarDecSyntax ReadClassVarDec()
        {
            Token kind = tokenizer.Read(TokenType.Keyword);
            Token type = tokenizer.Read();
            if (!isTypeToken(type))
                throw new ExpectedException("Not a type token", type);

            IReadOnlyList<Token> delimitedNames = tokenizer.ReadDelimitedList(
                readItem: () => tokenizer.Read(TokenType.Identifier),
                delimiter: ",",
                terminator: ";")
                .AsReadOnly();
            Token semicolon = tokenizer.Read(";");

            return new ClassVarDecSyntax(
                KindKeyword: kind,
                Type: type,
                DelimitedNames: delimitedNames,
                Semicolon: semicolon);
        }

        private SubroutineDecSyntax ReadSubroutineDecSyntax()
        {
            Token kind = tokenizer.Read(TokenType.Keyword);
            Token returnType = tokenizer.Read();
            if (!isTypeToken(returnType) && !(returnType.Value == "void"))
                throw new ExpectedException("Not a type token", returnType);

            Token name = tokenizer.Read(TokenType.Identifier);
            Token openArgs = tokenizer.Read("(");
            ParameterListSyntax parameterList = ReadParameterList();
            Token closeArgs = tokenizer.Read(")");
            SubroutineBodySyntax subruotineBody = ReadSubroutineBody();

            return new SubroutineDecSyntax(
                KindKeyword: kind,
                ReturnType: returnType,
                Name: name,
                OpenArgs: openArgs,
                ParameterList: parameterList,
                CloseArgs: closeArgs,
                SubroutineBody: subruotineBody);
        }

        private SubroutineBodySyntax ReadSubroutineBody()
        {
            Token open = tokenizer.Read("{");
            Token close;
            List<VarDecSyntax> varDecs = new List<VarDecSyntax>();
            List<StatementSyntax> statementsList = new List<StatementSyntax>();
            while (true)
            {
                Token token = tokenizer.Read();

                if (token.Value == "}")
                {
                    close = token;
                    break;
                }
                else if (token.Value == "var")
                {
                    tokenizer.PushBack(token);
                    varDecs.Add(ReadVarDec());
                }
                else if (statementKeywords.Contains(token.Value))
                {
                    tokenizer.PushBack(token);
                    statementsList.Add(ReadStatement());
                }
            }
            StatementsSyntax statements = new StatementsSyntax(statementsList.AsReadOnly());
            return new SubroutineBodySyntax(open, varDecs, statements, close);
        }

        public VarDecSyntax ReadVarDec()
        {
            Token var = tokenizer.Read("var");
            Token type = tokenizer.Read();
            IReadOnlyList<Token> delimitedNames = tokenizer.ReadDelimitedList(
                readItem: () => tokenizer.Read(TokenType.Identifier),
                delimiter: ",",
                terminator: ";")
                .AsReadOnly();
            Token semicolon = tokenizer.Read(";");

            return new VarDecSyntax(
                KindKeyword: var,
                Type: type,
                DelimitedNames: delimitedNames,
                Semicolon: semicolon);
        }

        public StatementsSyntax ReadStatements()
        {
            IReadOnlyList<StatementSyntax> statements = tokenizer.ReadList((token) =>
                    {
                        if (token.Value != "}")
                        {
                            tokenizer.PushBack(token);
                            return ReadStatement();
                        }
                        return null;
                    }).AsReadOnly();

            return new StatementsSyntax(statements);
        }

        private StatementSyntax ReadStatement()
        {
            Token statementStart = tokenizer.Read();
            tokenizer.PushBack(statementStart);

            if (IsStartOfLet(statementStart)) return ReadLetStatement();
            if (IsStartOfDo(statementStart)) return ReadDoStatement();
            if (statementStart.Value == "if") return ReadIfStatement();
            if (statementStart.Value == "while") return ReadWhileStatement();
            if (statementStart.Value == "return") return ReadReturnStatement();

            throw new ExpectedException("Expected statement keyword", statementStart);
        }

        private bool IsStartOfLet(Token token)
        {
            if (token.Value == "let") return true;
            if (token.TokenType == TokenType.Identifier)
            {
                var a = tokenizer.TryReadNext();
                var b = tokenizer.TryReadNext();
                if (b != null) tokenizer.PushBack(b);
                if (a != null) tokenizer.PushBack(a);
                return b?.Value == "=" || b?.Value == "[";
            }
            return false;
        }

        private bool IsStartOfDo(Token token)
        {
            if (token.Value == "do") return true;
            if (token.TokenType == TokenType.Identifier)
            {
                var a = tokenizer.TryReadNext();
                var b = tokenizer.TryReadNext();
                if (b != null) tokenizer.PushBack(b);
                if (a != null) tokenizer.PushBack(a);
                return b?.Value == "(" || b?.Value == ".";
            }
            return false;
        }

        private LetStatementSyntax ReadLetStatement()
        {
            Token let = tokenizer.Read();
            if (let.Value != "let")
            {
                tokenizer.PushBack(let);
                let = new Token(TokenType.Keyword, "let", let.LineNumber, let.ColNumber);
            }

            Token varName = tokenizer.Read(TokenType.Identifier);
            Token eqOrBrace = tokenizer.Read();
            tokenizer.PushBack(eqOrBrace);

            Indexing index = null;
            if (eqOrBrace.Value == "[")
            {
                index = ReadIndexing();
            }
            eqOrBrace = tokenizer.Read("=");
            ExpressionSyntax expr = ReadExpression();
            Token semicolon = tokenizer.Read(";");

            return new LetStatementSyntax(
                Let: let,
                VarName: varName,
                Index: index,
                Eq: eqOrBrace,
                Value: expr,
                Semicolon: semicolon);
        }

        private IfStatementSyntax ReadIfStatement()
        {
            Token ifToken = tokenizer.Read("if");
            Token open = tokenizer.Read("(");
            ExpressionSyntax expr = ReadExpression();
            Token close = tokenizer.Read(")");
            Token trueOpen = tokenizer.Read("{");
            StatementsSyntax statements = ReadStatements();
            Token trueClose = tokenizer.Read("}");

            Token? endOrElse = tokenizer.TryReadNext();
            tokenizer.PushBack(endOrElse);
            ElseClause elseClause = null;
            if (endOrElse != null && endOrElse.Value == "else")
            {
                elseClause = ReadElseClause();
            }
            return new IfStatementSyntax(
                 If: ifToken,
                 Open: open,
                 Condition: expr,
                 Close: close,
                 OpenTrue: trueOpen,
                 TrueStatements: statements,
                 CloseTrue: trueClose,
                 ElseClause: elseClause);
        }

        private ElseClause ReadElseClause()
        {
            Token elseToken = tokenizer.Read("else");
            Token open = tokenizer.Read("{");
            StatementsSyntax statements = ReadStatements();
            Token close = tokenizer.Read("}");

            return new ElseClause(elseToken, open, statements, close);
        }

        private WhileStatementSyntax ReadWhileStatement()
        {
            Token whileToken = tokenizer.Read("while");
            Token open = tokenizer.Read("(");
            ExpressionSyntax expr = ReadExpression();
            Token close = tokenizer.Read(")");
            Token openStatements = tokenizer.Read("{");
            StatementsSyntax statements = ReadStatements();
            Token closeStatements = tokenizer.Read("}");

            return new WhileStatementSyntax(
                While: whileToken,
                Open: open,
                Condition: expr,
                Close: close,
                OpenStatements: openStatements,
                Statements: statements,
                CloseStatements: closeStatements);
        }

        private DoStatementSyntax ReadDoStatement()
        {
            Token doToken = tokenizer.Read();
            if (doToken.Value != "do")
            {
                tokenizer.PushBack(doToken);
                doToken = new Token(TokenType.Keyword, "do", doToken.LineNumber, doToken.ColNumber);
            }

            SubroutineCall subroutineCall = ReadSubroutineCall();
            Token semicolon = tokenizer.Read(";");

            return new DoStatementSyntax(doToken, subroutineCall, semicolon);
        }

        private ReturnStatementSyntax ReadReturnStatement()
        {
            Token returnToken = tokenizer.Read("return");
            Token endOrExpr = tokenizer.Read();
            tokenizer.PushBack(endOrExpr);

            ExpressionSyntax expr = null;
            if (endOrExpr.Value != ";")
                expr = ReadExpression();

            Token semicolon = tokenizer.Read(";");

            return new ReturnStatementSyntax(
                Return: returnToken,
                ReturnValue: expr,
                Semicolon: semicolon);
        }

        private Indexing ReadIndexing()
        {
            Token open = tokenizer.Read("[");
            ExpressionSyntax index = ReadExpression();
            Token close = tokenizer.Read("]");

            return new Indexing(open, index, close);
        }

        public SubroutineCall ReadSubroutineCall()
        {
            MethodObjectOrClass methodOrClass = null;
            Token subroutineName = tokenizer.Read(TokenType.Identifier);

            Token dotOrBrace = tokenizer.Read();
            tokenizer.PushBack(dotOrBrace);
            if (dotOrBrace.Value == ".")
            {
                Token dot = tokenizer.Read(".");
                methodOrClass = new MethodObjectOrClass(subroutineName, dot);
                subroutineName = tokenizer.Read(TokenType.Identifier);
            }
            Token open = tokenizer.Read("(");
            ExpressionListSyntax arguments = ReadExpressionsList();
            Token close = tokenizer.Read(")");

            return new SubroutineCall(
                ObjectOrClass: methodOrClass,
                SubroutineName: subroutineName,
                Open: open,
                Arguments: arguments,
                Close: close);
        }

        private ExpressionListSyntax ReadExpressionsList()
        {
            List<ExpressionSyntax> exprList = tokenizer.ReadDelimitedList(
                readItem: () => ReadExpression(),
                delimiter: ",",
                terminator: ")");

            return new ExpressionListSyntax(exprList);
        }

        public ParameterListSyntax ReadParameterList()
        {
            IReadOnlyList<Parameter> delimitedParams = tokenizer.ReadDelimitedList(
                readItem: () =>
                {
                    Token type = tokenizer.Read();
                    Token name = tokenizer.Read(TokenType.Identifier);
                    if (!isTypeToken(type))
                        throw new ExpectedException("Expected type token", type);

                    return new Parameter(type, name);
                },
                delimiter: ",",
                terminator: ")")
                .AsReadOnly();

            return new ParameterListSyntax(delimitedParams);
        }

        public ExpressionSyntax ReadExpression()
        {
            TermSyntax term = ReadTerm();
            List<ExpressionTail> tailList = new List<ExpressionTail>();

            TermSyntax tailTerm;
            while (true)
            {
                Token op = tokenizer.TryReadNext();
                if (op == null)
                    break;

                if (!opSet.Contains(op.Value))
                {
                    tokenizer.PushBack(op);
                    break;
                }
                tailTerm = ReadTerm();
                tailList.Add(new ExpressionTail(op, tailTerm));
            }

            return new ExpressionSyntax(term, tailList.AsReadOnly());
        }

        public TermSyntax ReadTerm()
        {
            Token token = tokenizer.Read();
            if (unaryOpSet.Contains(token.Value))
            {
                return new UnaryOpTermSyntax(token, ReadTerm());
            }

            tokenizer.PushBack(token);
            if (token.Value == "(")
            {
                return ReadParenthesizedTerm();
            }

            if (isValueTermToken(token))
            {
                return ReadValueOrSubroutineCall();
            }

            throw new ExpectedException("Expected term tocken", token);
        }

        private ParenthesizedTermSyntax ReadParenthesizedTerm()
        {
            Token open = tokenizer.Read("(");
            ExpressionSyntax expr = ReadExpression();
            Token close = tokenizer.Read(")");
            return new ParenthesizedTermSyntax(open, expr, close);
        }

        private bool isValueTermToken(Token token)
        {
            if (token.TokenType != TokenType.Symbol && token.TokenType != TokenType.Keyword)
            {
                return true;
            }
            else if (keywordConstant.Contains(token.Value))
            {
                return true;
            }

            return false;
        }

        private TermSyntax ReadValueOrSubroutineCall()
        {
            Token firstToken = tokenizer.Read();
            Token? nextToken = tokenizer.TryReadNext();
            tokenizer.PushBack(nextToken);

            if (nextToken != null)
            {
                if (nextToken.Value == "(" || nextToken.Value == ".")
                {
                    tokenizer.PushBack(firstToken);
                    return new SubroutineCallTermSyntax(ReadSubroutineCall());
                }
                if (nextToken.Value == "[")
                {
                    return new ValueTermSyntax(firstToken, ReadIndexing());
                }
            }

            return new ValueTermSyntax(firstToken, null);
        }
    }
}
