using System;
using System.Collections.Generic;

namespace ShowPicture
{
    public static class ShowPictureTask
    {
        public static string[] GenerateShowPictureCode(bool[,] pixels)
        {
            List<string> asmCode = new List<string>();

            asmCode.Add("@SCREEN");
            asmCode.Add("D=A");
            asmCode.Add("@addr");
            asmCode.Add("M=D");

            int height = pixels.GetLength(0);
            int width = pixels.GetLength(1);

            short word = 0;
            int wordLen = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < 512; x += 16)
                {
                    word = 0;
                    wordLen = 0;
                    for (int b = Math.Min(16, width - x) - 1; b >= 0; b--)
                    {
                        word *= 2;
                        if (pixels[y, x + b]) { word++; }
                        wordLen++;
                    }

                    if (word == -32768)
                    {
                        asmCode.Add("@32767");
                        asmCode.Add("D = A + 1");
                    }
                    else if (word < 0)
                    {
                        asmCode.Add($"@{-word}");
                        asmCode.Add("D=A");
                        asmCode.Add("D = -D");
                    }
                    else
                    {
                        asmCode.Add($"@{word}");
                        asmCode.Add("D=A");
                    }

                    asmCode.Add("@addr");
                    asmCode.Add("A=M");
                    asmCode.Add("M=D");

                    asmCode.Add("@addr");
                    asmCode.Add("M=M+1");
                }
            }

            return asmCode.ToArray();
        }
    }
}
