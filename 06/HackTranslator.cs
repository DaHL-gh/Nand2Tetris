namespace Assembler
{
    public class HackTranslator
    {
        private int nextFreeMemory = 16;

        /// <summary>
        /// Транслирует инструкции ассемблерного кода (без меток) в бинарное представление.
        /// </summary>
        /// <param name="instructions">Ассемблерный код без меток</param>
        /// <param name="symbolTable">Таблица символов</param>
        /// <returns>Строки инструкций в бинарном формате</returns>
        /// <exception cref="FormatException">Ошибка трансляции</exception>
        public string[] TranslateAsmToHack(string[] instructions, Dictionary<string, int> symbolTable)
        {
            List<string> hackProgram = new List<string>();

            foreach (string instruction in instructions)
            {
                if (instruction.StartsWith("@"))
                {
                    hackProgram.Add(AInstructionToCode(instruction, symbolTable));
                }
                else
                {
                    hackProgram.Add(CInstructionToCode(instruction));
                }
            }

            return hackProgram.ToArray();
        }

        /// <summary>
        /// Транслирует одну A-инструкцию ассемблерного кода в бинарное представление
        /// </summary>
        /// <param name="aInstruction">Ассемблерная A-инструкция, например, @42 или @SCREEN</param>
        /// <param name="symbolTable">Таблица символов</param>
        /// <returns>Строка, содержащее нули и единицы — бинарное представление ассемблерной 
				/// инструкции, например, "0000000000000101"</returns>
        public string AInstructionToCode(string aInstruction, Dictionary<string, int> symbolTable)
        {
            string aParam = aInstruction.Substring(1);
            int aInt;
            if (symbolTable.ContainsKey(aParam))
            {
                aInt = symbolTable[aParam];
            }
            else
            {
                bool isNumber = int.TryParse(aParam, out aInt);
                if (!isNumber)
                {
                    aInt = nextFreeMemory;
                    symbolTable[aParam] = nextFreeMemory;
                    nextFreeMemory++;
                }
            }

            return Convert.ToString(aInt, 2).PadLeft(16, '0');
        }

        /// <summary>
        /// Транслирует одну C-инструкцию ассемблерного кода в бинарное представление
        /// </summary>
        /// <param name="cInstruction">Ассемблерная C-инструкция, например, A=D+M</param>
        /// <returns>Строка, содержащее нули и единицы — бинарное представление ассемблерной 
				/// инструкции, например, "1111000010100000"</returns>
        public string CInstructionToCode(string cInstruction)
        {
            string[] splitByColumn = cInstruction.Split(';', 2);
            string jump;
            string compAndDest = splitByColumn[0];
            if (splitByColumn.Length == 2)
            {
                jump = splitByColumn[1];
            }
            else
            {
                jump = "null";
            }

            string[] splitByEqSign = compAndDest.Split("=", 2);
            string comp;
            string dest;
            if (splitByEqSign.Length == 2)
            {
                dest = splitByEqSign[0];
                comp = splitByEqSign[1];
            }
            else
            {
                dest = "null";
                comp = splitByEqSign[0];
            }

            string a = comp.Contains("M") ? "1" : "0";
            return "111" + a + encodeComp(comp) + encodeDest(dest) + encodeJump(jump);
        }

        private string encodeDest(string dest)
        {
            string Mbit = dest.Contains("M") ? "1" : "0";
            string Dbit = dest.Contains("D") ? "1" : "0";
            string Abit = dest.Contains("A") ? "1" : "0";

            string binDest = Abit + Dbit + Mbit;
            return binDest;
        }

        private Dictionary<string, string> compTable = new Dictionary<string, string>
                    {
                            { "0", "101010" },
                            { "1", "111111" },
                            { "-1", "111010" },
                            { "D", "001100" },
                            { "A", "110000" },
                            { "!D", "001101" },
                            { "!A", "110001" },
                            { "-D", "001111" },
                            { "-A", "110011" },
                            { "D+1", "011111" },
                            { "A+1", "110111" },
                            { "D-1", "001110" },
                            { "A-1", "110010" },
                            { "D+A", "000010" },
                            { "D-A", "010011" },
                            { "A-D", "000111" },
                            { "D&A", "000000" },
                            { "D|A", "010101" },
                    };


        private string encodeComp(string comp)
        {
            comp = comp.Replace('M', 'A');

            string binComp = compTable[comp];

            return binComp;
        }

        private Dictionary<string, string> jumpTable = new Dictionary<string, string>
                {
                        {"null", "000"},
                        {"JGT", "001"},
                        {"JEQ", "010"},
                        {"JGE", "011"},
                        {"JLT", "100"},
                        {"JNE", "101"},
                        {"JLE", "110"},
                        {"JMP", "111"},
                };

        private string encodeJump(string comp)
        {
            string binJump = jumpTable[comp];

            return binJump;
        }
    }
}
