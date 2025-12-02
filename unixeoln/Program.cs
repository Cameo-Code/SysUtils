using System;
using System.IO;

namespace PasswHasher
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("please provide a file name");
                return 1;
            }
            // todo: 
            string inputFn = args[0];
            //string outputPath = args.Length == 2 ? args[1] : inputPath + ".tmp";
            string outputFn = inputFn;

            if (!File.Exists(inputFn))
            {
                Console.Error.WriteLine("Error: input file does not exist: " + inputFn);
                return 2;
            }

            try
            {
                byte[] data = File.ReadAllBytes(inputFn);

                using (var ms = new MemoryStream(data.Length))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        byte b = data[i];

                        // If CRLF (0D 0A), keep only LF
                        if (b == 0x0D)
                        {
                            // If next is 0A, skip CR
                            if (i + 1 < data.Length && data[i + 1] == 0x0A)
                            {
                                // Skip CR (0D). LF will be written by next iteration.
                                continue;
                            }
                            else
                            {
                                // Lone CR — convert to LF (0A)
                                ms.WriteByte(0x0A);
                                continue;
                            }
                        }

                        ms.WriteByte(b);
                    }

                    File.WriteAllBytes(outputFn, ms.ToArray());
                }

                // If we convert in-place — swap files
                // if (args.Length == 1)
                // {
                //     string backup = inputPath + ".bak";
                // 
                //     if (File.Exists(backup))
                //         File.Delete(backup);
                // 
                //     File.Move(inputPath, backup);
                //     File.Move(outputPath, inputPath);
                // 
                // }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return 3;
            }
        }
    }
}
