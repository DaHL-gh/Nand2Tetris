namespace Assembler
{
    public class Parser
    {
        /// <summary>
        /// Удаляет все комментарии и пустые строки из программы. Удаляет все пробелы
        /// из команд.
        /// </summary>
        /// <param name="asmLines">Строки ассемблерного кода</param>
        /// <returns>Только значащие строки строки ассемблерного кода без комментариев
        /// и лишних пробелов</returns>
        public string[] RemoveWhitespacesAndComments(string[] asmLines)
        {
            List<string> returnList = new List<string>();

            foreach (string line in asmLines)
            {
                string clrLine =
                    line.Split(new[] { "//" }, 2, StringSplitOptions.None)[0].Trim();

                clrLine = clrLine.Replace(" ", "");

                if (!string.IsNullOrEmpty(clrLine))
                {
                    returnList.Add(clrLine);
                }
            }

            return returnList.ToArray();
        }
    }
}
