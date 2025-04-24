using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Вставляет вызов функции Sys.init без аргументов
    /// </summary>
    public void WriteSysInitCall()
    {
        TryWriteFunctionCallCode(new VmInstruction(0, "call", "Sys.init", "0"));
    }

    /// <summary>
    /// Транслирует инструкции: call, function, return
    /// </summary>
    private bool TryWriteFunctionCallCode(VmInstruction instruction)
    {
        string name = instruction.Name;

        string functionName;
        int nArgs;
        switch (name)
        {
            case "call":
                functionName = instruction.Args[0];
                nArgs = int.Parse(instruction.Args[1]);

                WriteFunctionCall(functionName, nArgs);
                break;
            case "tailrec":
                functionName = instruction.Args[0];
                nArgs = int.Parse(instruction.Args[1]);

                WriteTailrec(functionName, nArgs);
                break;
            case "function":
                functionName = instruction.Args[0];
                int nVars = int.Parse(instruction.Args[1]);

                WriteFunctionInit(functionName, nVars);
                break;
            case "return":
                WriteFunctionReturn();
                break;
            default: return false;
        }
        return true;
    }

    private void WriteFunctionCall(string functionName, int nArgs)
    {
        string returnLabel = GenerateUniqueReturnLabel(functionName);
        WriteAsm("@" + returnLabel, "D=A");
        WritePushD();
        // saving old frame
        foreach (string label in new[] { "@LCL", "@ARG", "@THIS", "@THAT" })
        {
            WriteAsm(label, "D=M");
            WritePushD();
        }

        // calculating arg pointer
        WriteAsm($"@{5 + nArgs}", "D=A", "@SP", "D=M-D", "@ARG", "M=D");
        // LCL = SP
        WriteAsm("@SP", "D=M", "@LCL", "M=D");

        // go to function
        WriteAsm("@" + functionName, "0;JMP");
        WriteAsm($"({returnLabel})");
    }

    private void WriteFunctionInit(string functionName, int nVars)
    {
        WriteAsm($"({functionName})");
        for (int i = 0; i < nVars; i++)
        {
            WriteAsm("@0", "D=A");
            WritePushD();
        }
    }

    private void WriteFunctionReturn()
    {
        // store return address to @13
        WriteAsm("@5", "D=A", "@LCL", "A=M-D", "D=M", "@13", "M=D");

        // return value to top of stack
        WritePopToD();
        WriteAsm("@ARG", "A=M", "M=D");

        // restore SP
        WriteAsm("@ARG", "D=M", "@SP", "M=D+1");

        // restore THAT, THIS, ARG, LCL
        foreach (string label in new[] { "@THAT", "@THIS", "@ARG", "@LCL" })
        {
            WriteAsm("@LCL", "M=M-1", "D=M", "A=D", "D=M", label, "M=D");
        }

        // jump to return
        WriteAsm("@13", "A=M", "0;JMP");
    }

    private void WriteTailrec(string functionName, int nArgs)
    {
        // ARG += nArgs
        WriteAsm($"@{nArgs}", "D=A", "@ARG", "M=M+D");
        for (int i = 0; i < nArgs; i++)
        {
            WritePopToD();
            WriteAsm("@ARG", "A=M", "M=D");
            WriteAsm("@ARG", "M=M-1");
        }

        // SP = LCL
        WriteAsm("@LCL", "D=M", "@SP", "M=D");

        WriteAsm("@" + functionName, "0;JMP");
    }

    private int callCount = 0;
    private string GenerateUniqueReturnLabel(string functionName)
    {
        return $"{functionName}$ret.{callCount++}";
    }
}
