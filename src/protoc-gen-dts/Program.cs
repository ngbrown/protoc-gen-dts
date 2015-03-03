namespace Google.ProtocolBuffers.ProtoGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Google.ProtocolBuffers;
    using Google.ProtocolBuffers.Compiler.PluginProto;
    using Google.ProtocolBuffers.DescriptorProtos;

    class Program
    {
        public static int Main(string[] args)
        {
            DescriptorProtoFile.Descriptor.ToString();
            ExtensionRegistry extensionRegistry = ExtensionRegistry.CreateInstance();

            CodeGeneratorRequest request;
            var response = new CodeGeneratorResponse.Builder();
            try
            {
                if (IsPipedInput() == false || args.Length > 0)
                {
                    if (args.Length == 0 || File.Exists(args[0]) == false)
                    {
                        Console.Error.WriteLine(GetHelp(null));
                        return 1;
                    }
                    else
                    {
                        // read recorder request for debugging purposes
                        using (var fs = File.OpenRead(args[0]))
                        {
                            request = CodeGeneratorRequest.ParseFrom(fs);
                        }
                    }
                }
                else
                {
                    using (var input = Console.OpenStandardInput())
                    {
                        request = CodeGeneratorRequest.ParseFrom(input, extensionRegistry);
                    }
                }

                Run(request, response);
            }
            catch (Exception ex)
            {
                response.Error += ex.ToString();
            }

            using (var output = Console.OpenStandardOutput())
            {
                response.Build().WriteTo(output);
                output.Flush();
            }

            return 0;
        }

        private static void SaveRequest(CodeGeneratorRequest request, string path)
        {
            using (var fs = File.Create(path))
            {
                request.WriteTo(fs);
            }
        }

        private static void Run(CodeGeneratorRequest request, CodeGeneratorResponse.Builder response)
        {
            var arguments = new List<string>();
            foreach (var arg in request.Parameter.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                arguments.Add(arg);
            }

            GeneratorOptions options = new GeneratorOptions
            {
                Arguments = arguments
            };
            IList<string> validationFailures;
            if (!options.TryValidate(out validationFailures))
            {
                response.Error += BuildMessage(validationFailures);
                return;
            }

            if (options.SaveRequest != null)
            {
                SaveRequest(request, options.SaveRequest);
            }

            Generator generator = Generator.CreateGenerator(options);
            generator.Generate(request, response);
        }

        private static string GetHelp(List<string> tmpReasons)
        {
            if (tmpReasons == null)
            {
                tmpReasons = new List<string>();
            }

            tmpReasons.Add("This is a plugin to the protoc command.  Call by passing the argument '--dts_out[=<outdir>]' to protoc.");
            tmpReasons.Add("");
            tmpReasons.Add("You may specify parameters on the command-line by placing it before the output directory, separated by a colon:");
            tmpReasons.Add("  protoc --dts_out=enable_bar:outdir");
            tmpReasons.Add("");
            tmpReasons.Add("Arguments:");
            tmpReasons.Add("  saverequest=<filename>   save the CodeGeneratorRequest to a file for debugging.");
            tmpReasons.Add("  combined=<true|false>    combines into one file.  Default is false.  If true, specify output.");
            tmpReasons.Add("  output=<filename>        defaults to the namespace or protobuf.d.ts.");
            tmpReasons.Add("  namespace=<someApi>      defaults to no wrapping module");
            
            return BuildMessage(tmpReasons);
        }

        private static string BuildMessage(IEnumerable<string> reasons)
        {
            StringBuilder builder = new StringBuilder("Invalid options:");
            builder.AppendLine();
            foreach (string reason in reasons)
            {
                builder.AppendLine(reason);
            }

            return builder.ToString();
        }

        private static bool IsPipedInput()
        {
            try
            {
                bool isKey = Console.KeyAvailable;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
