using System;
using System.Collections.Generic;
using System.Linq;

namespace VMTranslator;

public class Parser {
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmLines) {
        List<VmInstruction> returnList = new List<VmInstruction>();

        int lineNum = 0;
        foreach (string line in vmLines) {
            string[] instructions = line.Split("\n");

            foreach (string instruction in instructions) {
                lineNum++;

                string noCommentsInstruction = instruction.Split("//", 2)[0];
                if (string.IsNullOrWhiteSpace(noCommentsInstruction)) { continue; }

                string[] splittedInstruction = noCommentsInstruction.Split(" ");
                splittedInstruction = splittedInstruction.Where(str => str.Length != 0).ToArray();

                string name = splittedInstruction[0];
                string[] arguments = splittedInstruction.AsSpan(1).ToArray();

                returnList.Add(new VmInstruction(LineNumber: lineNum, Name: name, Args: arguments));
            }
        }

        return returnList.ToArray();
    }
}
