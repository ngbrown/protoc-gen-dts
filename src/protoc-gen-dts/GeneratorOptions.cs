namespace Google.ProtocolBuffers.ProtoGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class GeneratorOptions
    {
        public List<string> Arguments { get; set; }
        public string SaveRequest { get; set; }
        public bool Combined { get; set; }
        public string OutputPath { get; set; }
        public string Namespace { get; set; }
        public string ConverterPath { get; set; }

        private const string DefaultOutputPath = "protobuf.d.ts";
        private const string DefaultConverterPath = "dtsconverters.yaml";

        public GeneratorOptions()
        {
            // set defaults
            this.SaveRequest = null;
            this.Combined = false;
            this.OutputPath = null;
            this.Namespace = null;
        }

        public bool TryValidate(out IList<string> reasons)
        {
            List<string> tmpReasons = new List<string>();

            ParseArguments(tmpReasons);

            if (this.Combined && string.IsNullOrEmpty(this.OutputPath))
            {
                if (string.IsNullOrEmpty(this.Namespace) == false)
                {
                    this.OutputPath = this.Namespace;
                }
                else
                {
                    this.OutputPath = DefaultOutputPath;
                }
            }

            if (this.ConverterPath == null && File.Exists(DefaultConverterPath))
            {
                this.ConverterPath = DefaultConverterPath;
            }

            if (tmpReasons.Count != 0)
            {
                reasons = tmpReasons;
                return false;
            }

            reasons = null;
            return true;
        }

        private static readonly Regex ArgMatch = new Regex(@"^(?:-|--|/)?(?<name>[\w_]+?)(?:=(?<value>.*))?$");

        private void ParseArguments(List<string> tmpReasons)
        {
            foreach (var argument in this.Arguments)
            {
                Match m = ArgMatch.Match(argument);

                if (m.Success)
                {
                    string name = m.Groups["name"].Value;
                    string value = m.Groups["value"].Value;

                    if (name.Equals("saverequest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            tmpReasons.Add("saverequest needs a filename");
                        }

                        this.SaveRequest = value;
                    }
                    else if (name.Equals("output", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.OutputPath = value;
                    }
                    else if (name.Equals("combined", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Combined = (value == string.Empty) || Convert.ToBoolean(value);
                    }
                    else if (name.Equals("namespace", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Namespace = value;
                    }
                    else if (name.Equals("converter", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (File.Exists(value) == false)
                        {
                            tmpReasons.Add("Specified converter file '" + value + "' does not exist.");
                        }

                        this.ConverterPath = value;
                    }
                    else
                    {
                        tmpReasons.Add("Unknown argument '" + name + "'.");
                    }
                }
                else
                {
                    tmpReasons.Add("Unknown argument format '" + argument + "'.");
                }
            }
        }
    }
}