using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// class Name { ... }
        /// </summary>
        public void WriteClass(ClassSyntax classSyntax)
        {
            currentClassName = classSyntax.Name.Value;
            FillClassSymbolsTable(classSyntax.ClassVars);

            foreach (SubroutineDecSyntax subroutine in classSyntax.SubroutineDec)
            {
                WriteSubroutine(subroutine);
            }
        }

        private void FillClassSymbolsTable(IReadOnlyList<ClassVarDecSyntax> classVars)
        {
            int index = 0;
            foreach (ClassVarDecSyntax varDec in classVars)
            {
                VarKind kind = GetVarKind(varDec.KindKeyword);
                string type = varDec.Type.Value;

                foreach (Token nameToken in varDec.DelimitedNames)
                {
                    VarInfo varInfo = new VarInfo(kind: kind, index: index, type: type);
                    classSymbols[nameToken.Value] = varInfo;
                    index++;
                }
            }
        }

        private VarKind GetVarKind(Token kindKeyword)
        {
            return kindKeyword.Value switch
            {
                "static" => VarKind.Static,
                "field" => VarKind.Field,
                _ => throw new ExpectedException("Unexpected kind token for class var", kindKeyword)
            };
        }

        private void WriteSubroutine(SubroutineDecSyntax subroutine)
        {
            string functionName = $"{currentClassName}.{subroutine.Name.Value}";
            methodSymbols = CreateFunctionSymbolTable(subroutine);

            int localsCount = methodSymbols.Count - subroutine.ParameterList.DelimitedParameters.Count;
            Write($"function {functionName} {localsCount}");

            switch (subroutine.KindKeyword.Value)
            {
                case "method":
                    WriteMethod(subroutine);
                    break;
                case "function":
                    WriteFunction(subroutine);
                    break;
                case "constructor":
                    WriteConstructor(subroutine);
                    break;
                default:
                    Console.WriteLine($"WARNING unknown subroutine name {subroutine.KindKeyword.Value}");
                    break;
            }
        }

        private Dictionary<string, VarInfo> CreateFunctionSymbolTable(SubroutineDecSyntax subroutine)
        {
            Dictionary<string, VarInfo> functionVars = new Dictionary<string, VarInfo>();
            int index = 0;
            if (subroutine.KindKeyword.Value == "method")
                index++;

            foreach (Parameter parameter in subroutine.ParameterList.DelimitedParameters)
            {
                functionVars[parameter.Name.Value] = new VarInfo(index++, VarKind.Parameter, parameter.Type.Value);
            }

            index = 0;
            foreach (VarDecSyntax varDec in subroutine.SubroutineBody.VarDec)
            {
                foreach (Token nameToken in varDec.DelimitedNames)
                {
                    functionVars[nameToken.Value] = new VarInfo(index++, VarKind.Local, varDec.Type.Value);
                }
            }

            return functionVars;
        }

        /// <summary>
        /// method Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteMethod(SubroutineDecSyntax subroutine)
        {
            Write("push argument 0");
            Write("pop pointer 0");
            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        /// <summary>
        /// function Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteFunction(SubroutineDecSyntax subroutine)
        {
            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        /// <summary>
        /// constructor Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteConstructor(SubroutineDecSyntax subroutine)
        {
            Write($"push constant {methodSymbols.Count}");
            Write("call Memory.alloc 1");
            Write("pop pointer 0");
            WriteStatements(subroutine.SubroutineBody.Statements);
        }


        /// <summary>
        /// ObjOrClassName . SubroutineName ( ExpressionList ) 
        /// </summary>
        private bool TryWriteSubroutineCall(TermSyntax term)
        {
            switch (term)
            {
                case SubroutineCallTermSyntax subroutineCall:
                    WriteSubroutineCall(subroutineCall.Call);
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// do SubroutineCall ; 
        /// </summary>
        private bool TryWriteDoStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case DoStatementSyntax doStatement:
                    WriteSubroutineCall(doStatement.SubroutineCall);
                    Write("pop temp 0");
                    return true;
                default: return false;
            }
        }

        private void WriteSubroutineCall(SubroutineCall call)
        {
            int argsCount = call.Arguments.DelimitedExpressions.Count;
            MethodObjectOrClass? objectOrClass = call.ObjectOrClass;
            string className;
            if (objectOrClass == null)
            {
                className = currentClassName;
                Write("push pointer 0");
                argsCount++;
            }
            else
            {
                VarInfo? varInfo = FindVarInfo(objectOrClass.Name.Value);
                if (varInfo != null)
                {
                    className = varInfo.Type;
                    Write($"push {varInfo.SegmentName} {varInfo.Index}");
                    argsCount++;
                }
                else
                {
                    className = objectOrClass.Name.Value;
                }
            }

            foreach (ExpressionSyntax expr in call.Arguments.DelimitedExpressions)
                WriteExpression(expr);

            string calleeName = className + "." + call.SubroutineName.Value;
            Write($"call {calleeName} {argsCount}");
        }

        /// <summary>
        /// return ;
        /// return Expression ;
        /// </summary>
        private bool TryWriteReturnStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case ReturnStatementSyntax returnStatement:
                    if (returnStatement.ReturnValue != null)
                        WriteExpression(returnStatement.ReturnValue);
                    else
                        Write("push temp 0");
                    Write("return");
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// this | null
        /// </summary>
        private bool TryWriteObjectValue(TermSyntax term)
        {
            switch (term)
            {
                case (ValueTermSyntax valueTerm):
                    if (valueTerm.Indexing != null)
                        return false;

                    if (valueTerm.Value.Value == "this")
                    {
                        Write("push pointer 0");
                        return true;
                    }
                    if (valueTerm.Value.Value == "null")
                    {
                        Write("push constant 0");
                        return true;
                    }

                    return false;
                default: return false;
            }
        }
    }
}
