using System.Collections.Generic;

namespace Assembler
{
    public class SymbolAnalyzer
    {
        /// <summary>
        /// Находит все метки в ассемблерном коде, удаляет их из кода и вносит их
        /// адреса в таблицу символов.
        /// </summary>
        /// <param name="instructionsWithLabels">Ассемблерный код, возможно,
        /// содержащий метки</param> <param
        /// name="instructionsWithoutLabels">Ассемблерный код без меток</param>
        /// <returns>
        /// Таблица символов, содержащая все стандартные предопределенные символы
        /// (R0−R15, SCREEN, ...), а также все найденные в программе метки.
        /// </returns>
        public Dictionary<string, int>
        CreateSymbolsTable(string[] instructionsWithLabels,
                           out string[] instructionsWithoutLabels)
        {
            Dictionary<string, int> symbolsTable = GetBaseSymbolsTable();
            List<string> instructionsWithoutLabelsList = new List<string>();

            int i = 0;
            foreach (string instruction in instructionsWithLabels)
            {
                if (instruction.StartsWith("(") && instruction.EndsWith(")"))
                {
                    string strippedInstruction = instruction.Trim('(', ')');

                    if (symbolsTable.ContainsKey(strippedInstruction))
                    {
                        throw new ArgumentException(
                            $"Label '{strippedInstruction}' already exists");
                    }

                    symbolsTable[strippedInstruction] = i;
                }
                else
                {
                    instructionsWithoutLabelsList.Add(instruction);
                    i++;
                }
            }

            instructionsWithoutLabels = instructionsWithoutLabelsList.ToArray();
            return symbolsTable;
        }

        private Dictionary<string, int> GetBaseSymbolsTable()
        {
            Dictionary<string, int> baseSymbolsTable = new Dictionary<string, int> {
      { "SP", 0 },   { "LCL", 1 },         { "ARG", 2 },     { "THIS", 3 },
      { "THAT", 4 }, { "SCREEN", 0x4000 }, { "KBD", 0x6000 }
    };

            for (int j = 0; j < 16; j++)
            {
                baseSymbolsTable["R" + j] = j;
            }
            return baseSymbolsTable;
        }
    }
}
