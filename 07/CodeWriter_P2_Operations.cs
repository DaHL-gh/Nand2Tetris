using System.Collections.Generic;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * арифметических операция: add sub, neg
    /// * логических операций: eq, gt, lt, and, or, not
    /// </summary>
    /// <returns>true − если это логическая или арифметическая инструкция, иначе — false.</returns>
    private bool TryWriteLogicAndArithmeticCode(VmInstruction instruction)
    {
        string name = instruction.Name;
        switch (name)
        {
            case "add":
            case "sub":
            case "and":
            case "or":
                TranslateBinaryOperations(name);
                break;
            case "eq":
            case "gt":
            case "lt":
                TranslateCompareOperations(name);
                break;
            case "not":
                WriteAsm("@SP", "A=M-1", "M=-M", "M=M-1");
                break;
            case "neg":
                WriteAsm("@SP", "A=M-1", "M=-M");
                break;
            default: return false;
        }
        return true;
    }

    private Dictionary<string, char> bitWiseOperatorsMapping = new Dictionary<string, char> {
            {"add", '+'},
            {"sub", '-'},
            {"and", '&'},
            {"or", '|'},};
    private void TranslateBinaryOperations(string name)
    {
        char bitWiseOperator = bitWiseOperatorsMapping[name];
        WritePopToD();
        WriteAsm("@SP", "A=M-1", $"M=M{bitWiseOperator}D");
    }

    private Dictionary<string, string> jumpConditionMapping = new Dictionary<string, string> {
            { "eq", "NE" },
            { "gt", "LE" },
            { "lt", "GE" }};
    private void TranslateCompareOperations(string name)
    {
        string compareEnd = GenerateUniqueLabel("LT_END");
        string jumpCondiiton = jumpConditionMapping[name];

        WritePopToD();
        WriteAsm("@SP", "A=M-1", "D=M-D", "M=0");

        WriteAsm("@" + compareEnd, $"D;J{jumpCondiiton}");
        WriteAsm("@SP", "A=M-1", "M=-1");

        WriteAsm($"({compareEnd})");
    }

    private int labelCounter = 0;
    private string GenerateUniqueLabel(string label_base)
    {
        return $"{label_base}_{labelCounter++}";
    }
}
