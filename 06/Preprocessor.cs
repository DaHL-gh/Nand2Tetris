using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace Assembler
{
    public class Preprocessor
    {
        /// <summary>
        /// Преобразует нестандартные макро-инструкции в инструкции обычного языка ассемблера.
        /// </summary>
        public string[] PreprocessAsm(string[] instructions)
        {
            var asmCode = new List<string>();
            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                try
                {
                    TranslateInstruction(instr, asmCode);
                }
                catch (Exception e)
                {
                    throw new FormatException($"Can't parse at line {i + 1}: {instr}", e);
                }
            }

            return asmCode.ToArray();
        }

        public void TranslateInstruction(string instruction, List<string> asmCode)
        {
            Match match = Regex.Match(instruction, @"\[(.+?)\]");
            string instr_wo_match = Regex.Replace(instruction, @"\[.+?\]", "");

            if (match.Success)
            {
                asmCode.Add("@" + match.Groups[1].Value);
            }

            if (instr_wo_match[0] == 'J' && instr_wo_match.Length == 3)
            {
                if (instr_wo_match == "JMP")
                {
                    asmCode.Add("0;JMP");
                }
                else
                {
                    asmCode.Add("D;" + instr_wo_match);
                }
            }
            else
            {
                asmCode.Add(instr_wo_match);
            }
        }
    }
}
