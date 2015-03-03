namespace Google.ProtocolBuffers.ProtoGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Google.ProtocolBuffers.Compiler.PluginProto;
    using Google.ProtocolBuffers.DescriptorProtos;

    using YamlDotNet.Serialization.NamingConventions;

    public class Generator
    {
        private readonly GeneratorOptions options;
        private readonly Dictionary<string, string> converters;

        private Generator(GeneratorOptions options, Dictionary<string, string> converters)
        {
            this.options = options;
            this.converters = converters;
        }

        public static Generator CreateGenerator(GeneratorOptions options)
        {
            var converters = new Dictionary<string, string>();
            if (options.ConverterPath != null)
            {
                using (var sr = File.OpenText(options.ConverterPath))
                {
                    var deserializer = new YamlDotNet.Serialization.Deserializer(null, new NullNamingConvention(), false);
                    converters = deserializer.Deserialize<Dictionary<string, string>>(sr);
                }
            }

            return new Generator(options, converters);
        }

        public void Generate(CodeGeneratorRequest request, CodeGeneratorResponse.Builder response)
        {
            var descriptors = request.ProtoFileList;

            bool firstDescriptor = true;
            foreach (var descriptor in descriptors)
            {
                var indent = 0;
                var descriptorName = RemoveEnding(descriptor.Name, ".proto");

                var sb = new StringBuilder();

                var responseFile = CodeGeneratorResponse.Types.File.CreateBuilder();

                if (options.Combined == false)
                {
                    responseFile.SetName(descriptorName + ".d.ts");
                }
                else if (firstDescriptor)
                {
                    responseFile.SetName(RemoveEnding(options.OutputPath, ".d.ts") + ".d.ts");
                }

                if (this.options.Combined == false || firstDescriptor)
                {
                    sb.Append("// Generated with protoc-gen-ts.");
                    if (request.HasParameter)
                    {
                        sb.AppendFormat(" Parameters: \"{0}\"", request.Parameter);
                    }

                    sb.AppendLine("  DO NOT EDIT!");
                }

                sb.AppendLine();

                if (this.options.Combined)
                {
                    sb.AppendLine(string.Format("// Next section generated from \"{0}\".", descriptor.Name));
                    sb.AppendLine();
                }

                if (options.Namespace != null)
                {
                    sb.AppendLine(string.Format("declare module {0}", options.Namespace));
                    sb.AppendLine("{");
                    indent++;
                }

                if (request.FileToGenerateList.Contains(descriptor.Name) == false)
                {
                    // we weren't asked to generate a file for this filedescriptor.
                    continue;
                }

                //if (!firstDescriptor)
                //{
                //    sb.AppendLine();
                //}

                bool firstChild = true;
                BuildEnumList(descriptor.EnumTypeList, sb, indent, ref firstChild);
                BuildMessageList(descriptor.MessageTypeList, sb, indent, ref firstChild);

                indent = 0;

                if (options.Namespace != null)
                {
                    sb.AppendLine("}");
                }

                responseFile.SetContent(sb.ToString());

                response.AddFile(responseFile);

                firstDescriptor = false;
            }
        }

        private static string RemoveEnding(string value, string unwantedEnding)
        {
            if (value.EndsWith(unwantedEnding, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - unwantedEnding.Length);
            }

            return value;
        }

        private static void BuildEnumList(IList<EnumDescriptorProto> enumTypeList, StringBuilder sb, int indent, ref bool firstChild)
        {
            foreach (var enumType in enumTypeList)
            {
                if (!firstChild)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(Indent(indent) + string.Format("enum {0} {{", enumType.Name));

                bool firstValue = true;
                foreach (var value in enumType.ValueList)
                {
                    if (!firstValue)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append(Indent(indent) + string.Format("    {0} = {1}", value.Name, value.Number));


                    firstValue = false;
                }

                sb.AppendLine();
                sb.AppendLine(Indent(indent) + "}");

                firstChild = false;
            }
        }

        private static string Indent(int indent)
        {
            return new String(' ', indent * 4);
        }

        private void BuildMessageList(IList<DescriptorProto> messageTypeList, StringBuilder sb, int indent, ref bool firstChild)
        {
            foreach (var message in messageTypeList)
            {
                if (message.NestedTypeCount > 0 || message.EnumTypeCount > 0)
                {
                    if (!firstChild)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(Indent(indent) + string.Format("module {0} {{", message.Name));

                    bool firstNestedChild = true;
                    BuildEnumList(message.EnumTypeList, sb, indent + 1, ref firstNestedChild);
                    BuildMessageList(message.NestedTypeList, sb, indent + 1, ref firstNestedChild);

                    sb.AppendLine(Indent(indent) + "}");

                    firstChild = false;
                }

                if (!firstChild)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(Indent(indent) + string.Format("interface {0} {{", message.Name));

                foreach (var field in message.FieldList)
                {
                    string type = "any";
                    bool documentOriginalType = true;
                    switch (field.Type)
                    {
                        case FieldDescriptorProto.Types.Type.TYPE_DOUBLE:
                        case FieldDescriptorProto.Types.Type.TYPE_FLOAT:
                        case FieldDescriptorProto.Types.Type.TYPE_INT64:
                        case FieldDescriptorProto.Types.Type.TYPE_UINT64:
                        case FieldDescriptorProto.Types.Type.TYPE_INT32:
                        case FieldDescriptorProto.Types.Type.TYPE_FIXED64:
                        case FieldDescriptorProto.Types.Type.TYPE_FIXED32:
                        case FieldDescriptorProto.Types.Type.TYPE_UINT32:
                        case FieldDescriptorProto.Types.Type.TYPE_SFIXED32:
                        case FieldDescriptorProto.Types.Type.TYPE_SFIXED64:
                        case FieldDescriptorProto.Types.Type.TYPE_SINT32:
                        case FieldDescriptorProto.Types.Type.TYPE_SINT64:
                            type = "number";
                            break;
                        case FieldDescriptorProto.Types.Type.TYPE_BOOL:
                            type = "boolean";
                            documentOriginalType = false;
                            break;
                        case FieldDescriptorProto.Types.Type.TYPE_STRING:
                            type = "string";
                            documentOriginalType = false;
                            break;
                        case FieldDescriptorProto.Types.Type.TYPE_GROUP:
                            break;
                        case FieldDescriptorProto.Types.Type.TYPE_MESSAGE:
                        case FieldDescriptorProto.Types.Type.TYPE_ENUM:
                            type = field.TypeName.TrimStart('.');
                            documentOriginalType = false;
                            break;
                        case FieldDescriptorProto.Types.Type.TYPE_BYTES:
                            // will be sent as base64
                            type = "string";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("field.type", field.Type, "Unknown FieldDescriptorProto.Types.Type");
                    }

                    if (field.Type == FieldDescriptorProto.Types.Type.TYPE_MESSAGE && this.converters != null)
                    {
                        string requestedType;
                        if (this.converters.TryGetValue(type, out requestedType))
                        {
                            type = requestedType;
                            documentOriginalType = true;
                        }
                    }

                    if (documentOriginalType)
                    {
                        sb.Append(Indent(indent) + "    /** ");
                        sb.Append(field.Type);
                        if (field.HasTypeName)
                        {
                            sb.AppendFormat(", TypeName: {0}", field.TypeName);
                        }
                        sb.AppendLine(" */");
                    }

                    if (field.Label == FieldDescriptorProto.Types.Label.LABEL_REPEATED)
                    {
                        sb.Append(string.Format(Indent(indent) + "    {0}: Array<{1}>;", field.Name, type));
                    }
                    else
                    {
                        var required = field.Label == FieldDescriptorProto.Types.Label.LABEL_REQUIRED;
                        sb.Append(Indent(indent) + string.Format("    {0}{1}: {2};", field.Name, required ? string.Empty : "?", type));
                    }

                    sb.AppendLine();
                }

                sb.AppendLine(Indent(indent) + "}");

                firstChild = false;
            }
        }
    }
}