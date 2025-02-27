using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JackCompiling
{
    public class Tokenizer
    {
        private string text;
        public Tokenizer(string text)
        {
            this.text = text;
            textAfterCaret = text;

            PrecomputeNewLines();
        }

        private int caret = 0;
        private int line;
        private int column;
        private string textAfterCaret = "";
        private void AdvanceCaret(int length)
        {
            caret ++;
						CalculateLineAndColumn();
            caret += length - 1;
            textAfterCaret = text.Substring(caret);
        }

        private void CalculateLineAndColumn()
        {
            line = newLineIndeces.BinarySearch(caret);
            if (line < 0) this.line = ~line; // = -(line) -  1 | index of the first larger element

            column = caret - newLineIndeces[line - 1] - 1;
        }

        private readonly List<int> newLineIndeces = new List<int>{ -1 };
        private void PrecomputeNewLines()
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    newLineIndeces.Add(i);
            }
        }

        private Stack<Token> pushedBackTokens = new Stack<Token>();
        private List<Tuple<string, Func<string, int, int, Token>>> regexToFuncMapping =
                    new List<Tuple<string, Func<string, int, int, Token>>>
                    {
                    new Tuple<string, Func<string, int, int, Token>>(@"^\d+", ConvertIntegerConstant),
                    new Tuple<string, Func<string, int, int, Token>>("^\"[^\"\n]*\"", ConvertStringConstant),
                    new Tuple<string, Func<string, int, int, Token>>(@"^[{}()\[\].,;+\-*/&|<>~=]", ConvertSymbol),
                    new Tuple<string, Func<string, int, int, Token>>(@"^[a-zA-Z_][a-zA-Z0-9_]*", ConvertIdentifier),
                    };

        /// <summary>
        /// Сначала возвращает все токены, которые вернули методом PushBack в порядке First In Last Out.
        /// Потом читает и возвращает один следующий токен, либо null, если больше токенов нет.
        /// Пропускает пробелы и комментарии.
        ///
        /// Хорошо, если внутри Token сохранит ещё и строку и позицию в исходном тексте. Но это не проверяется тестами.
        /// </summary>
        public Token? TryReadNext()
        {
            if (pushedBackTokens.Count > 0)
                return pushedBackTokens.Pop();

            if (caret >= text.Length)
                return null;
            SkipWhiteSpacesAndComments();
            if (caret >= text.Length)
                return null;

            foreach (var (pattern, function) in regexToFuncMapping)
            {
                Match match = Regex.Match(textAfterCaret, pattern);

                if (match.Success)
                {
                    AdvanceCaret(match.Length);
                    return function(match.Value, line, column);
                }
            }

            return null;
        }

        private void SkipWhiteSpacesAndComments()
        {
            Match match = Regex.Match(textAfterCaret, @"^(//.*|\s|/\*[\s\S]*?\*/)*");

            if (match.Success)
                AdvanceCaret(match.Length);
        }

        private static Token ConvertIntegerConstant(string value, int line, int column)
        {
            return new Token(TokenType.IntegerConstant, value, line, column);
        }

        private static Token ConvertStringConstant(string value, int line, int column)
        {
            return new Token(TokenType.StringConstant, value.Substring(1, value.Length - 2), line, column);
        }

        private static Token ConvertSymbol(string value, int line, int column)
        {
            return new Token(TokenType.Symbol, value, line, column);
        }

        private static Token ConvertIdentifier(string value, int line, int column)
        {
            string pattern = @"^\b(class|constructor|function|method|field|static|var|int|char|boolean|void|true|false|null|this|let|do|if|else|while|return)\b";
            Match match = Regex.Match(value, pattern);
            TokenType tokenType = TokenType.Identifier;
            if (match.Success)
            {
                tokenType = TokenType.Keyword;
            }

            return new Token(tokenType, value, line, column);
        }

        /// <summary>
        /// Откатывает токенайзер на один токен назад.
        /// Если token - null, то игнорирует его и никуда не возвращает.
        /// Поддержка null нужна для удобства, чтобы использовать TryReadNext, вместе с PushBack без лишних if-ов.
        /// </summary>
        public void PushBack(Token? token)
        {
            if (token != null)
                pushedBackTokens.Push(token);
        }
    }
}
