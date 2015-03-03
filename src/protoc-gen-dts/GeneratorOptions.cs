namespace Google.ProtocolBuffers.ProtoGen
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class GeneratorOptions
    {
        public List<string> Arguments { get; set; }
        public string SaveRequest { get; set; }
        public bool Combined { get; set; }
        public string OutputPath { get; set; }
        public string Namespace { get; set; }

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
                    this.OutputPath = "protobuf.d.ts";
                }
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