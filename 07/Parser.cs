using System;
using System.Collections.Generic;

namespace VMTranslator;

public class Parser
{
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmLines)
    {
        List<VmInstruction> returnList = new List<VmInstruction>();

        int lineNum = 1;
        foreach (string instruction in vmLines)
        {
            string noCommentsInstruction = instruction.Split("//", 2)[0];
            if (noCommentsInstruction == "\n")
            {
                continue;
            }

            string[] splittedInstruction = noCommentsInstruction.Split(" ");
            
            string name = splittedInstruction[0];
            string[] arguments = splittedInstruction.AsSpan(1).ToArray();
						Console.WriteLine("|" + name + "\n");
						
            foreach (string str in arguments)
            {
                Console.WriteLine(str);
            }

            returnList.Add(new VmInstruction(LineNumber: lineNum, Name: name, Args: arguments));
            lineNum++;
        }

        return returnList.ToArray();
    }
}
