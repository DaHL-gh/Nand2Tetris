using System;
using System.Collections.Generic;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * push [segment] [index] — записывает на стек значение взятое из ячейки [index] сегмента [segment].
    /// * pop [segment] [index] — снимает со стека значение и записывает его в ячейку [index] сегмента [segment].
    ///
    /// Сегменты:
    /// + constant — виртуальный сегмент, по индексу [index] содержит значение [index]
    /// * local — начинается в памяти по адресу Ram[LCL]
    /// * argument — начинается в памяти по адресу Ram[ARG]
    /// * this — начинается в памяти по адресу Ram[THIS]
    /// * that — начинается в памяти по адресу Ram[THAT]
    /// * pointer - по индексу 0, содержит значение Ram[THIS], а по индексу 1 — значение Ram[THAT] 
    /// + temp - начинается в памяти по адресу 5
    /// + static — хранит значения по адресу, который ассемблер выделит переменной @{moduleName}.{index}
    /// </summary>
    /// <returns>
    /// true − если это инструкция работы со стеком, иначе — false.
    /// Если метод возвращает false, он не должен менять ResultAsmCode
    /// </returns>
    private bool TryWriteStackCode(VmInstruction instruction, string moduleName)
    {
        string segment;
        string index;
        if (instruction.Name == "push")
        {
            segment = instruction.Args[0];
            index = instruction.Args[1];
            return TranslatePushSegment(segment, index, moduleName);
        }
        else if (instruction.Name == "pop")
        {
            segment = instruction.Args[0];
            index = instruction.Args[1];
            return TransletePopSegment(segment, index, moduleName);
        }
        return false;
    }

    private Dictionary<string, string> segmentAddresses = new Dictionary<string, string> {
            { "local", "@LCL" },
            { "argument", "@ARG" },
            { "this", "@THIS" },
            { "that", "@THAT" } };

    private bool TranslatePushSegment(string segment, string index, string moduleName)
    {
        switch (segment)
        {
            case "constant":
                WriteAsm("@" + index, "D=A");
                break;
            case "local":
            case "argument":
            case "this":
            case "that":
                PushFromSegment(segment, index);
                break;
            case "pointer":
                string thisOrThat = index == "0" ? "THIS" : "THAT";
                WriteAsm(new[] { "@" + thisOrThat, "D=M" });
                break;
            case "temp":
                WriteAsm(new[] { $"@{5 + int.Parse(index)}", "D=M" });
                break;
            case "static":
                WriteAsm(new[] { $"@{moduleName}.{index}", "D=M" });
                break;
            default:
                return false;
        }
        WritePushD();
        return true;
    }

    private void PushFromSegment(string segment, string index)
    {
        string address = segmentAddresses[segment];
        WriteAsm(address, "D=M", "@" + index, "A=D+A", "D=M");
    }

    private bool TransletePopSegment(string segment, string index, string moduleName)
    {
        switch (segment)
        {
            case "local":
            case "argument":
            case "this":
            case "that":
                PopToSegment(segment, index);
                break;
            case "pointer":
                WritePopToD();
                string thisOrThat = index == "0" ? "THIS" : "THAT";
                WriteAsm(new[] { "@" + thisOrThat, "M=D" });
                break;
            case "temp":
                WritePopToD();
                WriteAsm(new[] { $"@{5 + int.Parse(index)}", "M=D" });
                break;
            case "static":
                WritePopToD();
                WriteAsm(new[] { $"@{moduleName}.{index}", "M=D" });
                break;
            default: return false;
        }
        return true;
    }

    private void PopToSegment(string segment, string index)
    {
        string address = segmentAddresses[segment];
        WriteAsm(address, "D=M", "@" + index, "D=D+A", "@13", "M=D");
        WritePopToD();
        WriteAsm("@13", "A=M", "M=D");
    }

    // Генерирует код, для сохранения значения D регистра в стек
    private void WritePushD()
    {
        string[] pushToDInstructoins = new[] { "@SP", "M=M+1", "A=M-1", "M=D" };
        WriteAsm(pushToDInstructoins);
    }

    // Генерирует код, для извлечения из стека значения в D регистр
    private void WritePopToD()
    {
        string[] popToDInstructoins = new[] { "@SP", "M=M-1", "A=M", "D=M" };
        WriteAsm(popToDInstructoins);
    }
}
