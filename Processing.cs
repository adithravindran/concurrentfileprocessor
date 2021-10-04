using System;
using System.Collections;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    public static class Processing
    {
        const int MAX_BUFFER = 1048576; //1MB 
        private static string outputPath = "C:\\New folder\\outputfiles\\";
        public static string transformAlgoWrapper(string file, string inputFilePath, TransformAlgoType type)
        {
            
            byte[] buffer = new byte[MAX_BUFFER];
            int bytesRead;
            string str = "";
            string processedFile = file;
            var fileName = Path.GetFileName(file).Split('.')[1];
            var groupName = Path.GetFileName(file).Split('.')[0];
            using (FileStream fs = File.Open((inputFilePath + file), FileMode.Open, FileAccess.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                while ((bytesRead = bs.Read(buffer, 0, MAX_BUFFER)) != 0) //reading 1mb chunks at a time
                {
                    if (type == TransformAlgoType.LowerCase)
                    {
                        // call lowercase transform private function
                        Console.WriteLine($"Processing lowercase for {inputFilePath + file}");
                        str = lowerCaseTransform(buffer);
                    }
                    else if (type == TransformAlgoType.UpperCase)
                    {
                        // call uppercase transform private function
                    }
                    
                    processedFile = outputPath + groupName + "."+ fileName + ".processed.txt";
                    StreamWriter sw = File.AppendText(processedFile);
                    sw.WriteLine(str);
                }
            }
            return processedFile;
        }

        private static string lowerCaseTransform(byte[] buffer)
        {
            return Encoding.Default.GetString(buffer).ToLower();            
        }
    }
}
