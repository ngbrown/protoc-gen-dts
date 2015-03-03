"..\packages\Google.ProtocolBuffers.2.4.1.555\tools\ProtoGen.exe" --include_imports -ignore_google_protobuf=true -namespace=Google.ProtocolBuffers.Compiler.PluginProto -file_extension=.Generated.cs google\protobuf\compiler\plugin.proto"

move /Y *.Generated.cs "..\protoc-gen-dts\"
