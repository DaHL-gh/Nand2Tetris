using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции: label, goto, if-goto
    /// </summary>
    private bool TryWriteProgramFlowCode(VmInstruction instruction, string moduleName)
    {
        string name = instruction.Name;

        string label;
        switch (name)
        {
            case "label":
                label = instruction.Args[0];
                WriteAsm($"({moduleName}.{label})");
                break;
            case "goto":
                label = instruction.Args[0];
                WriteAsm($"@{moduleName}.{label}", "0;JMP");
                break;
            case "if-goto":
                label = instruction.Args[0];
								WritePopToD();
                WriteAsm($"@{moduleName}.{label}", "D;JNE");
                break;
            default: return false;
        }
        return true;
    }
}
