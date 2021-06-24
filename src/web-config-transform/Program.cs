// This code is based on code supplied in a stack overflow answer given by Matt (https://stackoverflow.com/users/1016343/matt).
// https://stackoverflow.com/questions/8841075/web-config-transform-not-working

using System;
using System.Linq;
using Microsoft.Web.XmlTransform;

namespace web_config_transform
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var myDocumentsFolder = $@"C:\Users\{Environment.UserName}\Documents";
            var myVsProjects = $@"{myDocumentsFolder}\Visual Studio 2015\Projects";

            var srcConfigFileName = "Web.config";
            var tgtConfigFileName = srcConfigFileName;
            var transformFileName = "Web.Debug.config";
            var basePath = myVsProjects + @"\";
            
            try
            {

                var numArgs = args?.Length ?? 0;
                if (numArgs == 0 || args.Any(x=>x=="/?"))
                {
                    Console.WriteLine("\nTransformConfig - Usage:");
                    Console.WriteLine("\tTransformConfig.exe /d:tgtConfigFileName [/t:transformFileName [/s:srcConfigFileName][/b:basePath]]");
                    Console.WriteLine($"\nIf 'basePath' is just a directory name, '{basePath}' is preceded.");
                    Console.WriteLine("\nTransformConfig - Example (inside PostBuild event):");
                    Console.WriteLine("\t\"c:\\Tools\\TransformConfig.exe\"  /d:Web.config /t:Web.$(ConfigurationName).config /s:Web.Template.config /b:\"$(ProjectDir)\\\"");
                    Environment.ExitCode = 1;
                    return 1;
                }

                foreach (var a in args)
                {
                    var param = a.Trim().Substring(3).TrimStart();
                    switch (a.TrimStart().Substring(0,2).ToLowerInvariant())
                    {
                        case "/d":
                            tgtConfigFileName = param ?? tgtConfigFileName;
                            break;
                        case "/t":
                            transformFileName = param ?? transformFileName;
                            break;
                        case "/b":
                            var isPath = (param ?? "").Contains("\\");
                            basePath = (isPath == false)
                                        ? $@"{myVsProjects}\" + param ?? ""
                                        : param;
                            break;
                        case "/s":
                            srcConfigFileName = param ?? srcConfigFileName;
                            break;
                        default:
                            break;
                    }
                }
                basePath = System.IO.Path.GetFullPath(basePath);
                if (!basePath.EndsWith("\\")) basePath += "\\";
                if (tgtConfigFileName != srcConfigFileName)
                {
                    System.IO.File.Copy(basePath + srcConfigFileName,
                                         basePath + tgtConfigFileName, true);
                }
                TransformConfig(basePath + tgtConfigFileName, basePath + transformFileName);
                Console.WriteLine($"TransformConfig - transformed '{basePath + tgtConfigFileName}' successfully using '{transformFileName}'.");
                Environment.ExitCode = 0;
                return 0;
            }
            catch (Exception ex)
            {
                var msg = $"{ex.Message}\nParameters:\n/d:{tgtConfigFileName}\n/t:{transformFileName}\n/s:{srcConfigFileName}\n/b:{basePath}";
                Console.WriteLine($"TransformConfig - Exception occurred: {msg}");
                Console.WriteLine($"TransformConfig - Processing aborted.");
                Environment.ExitCode = 2;
                return 2;
            }
        }
        
        private static void TransformConfig(string configFileName, string transformFileName)
        {
            var document = new XmlTransformableDocument();
            document.PreserveWhitespace = true;
            document.Load(configFileName);

            var transformation = new XmlTransformation(transformFileName);
            if (!transformation.Apply(document))
            {
                throw new Exception("Transformation Failed");
            }
            document.Save(configFileName);
        }
    }
}
