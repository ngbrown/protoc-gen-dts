protoc-gen-dts
==============

This is a generator plugin for the Google Protocol Buffers compiler (`protoc`).  The plugin will generate TypeScript definition files (`*.d.ts`) from your `.proto` files.

## Usage

```
This is a plugin to the protoc command.  Call by passing the argument '--dts_out[=<outdir>]' to protoc.

You may specify parameters on the command-line by placing it before the output directory, separated by a colon:
  protoc --dts_out=enable_bar:outdir

Arguments:
  saverequest=<filename>   save the CodeGeneratorRequest to a file for debugging.
  combined=<true|false>    combines into one file.  Default is false.  If true, specify output.
  output=<filename[.d.ts]> defaults to the namespace or protobuf.d.ts.
  namespace=<someApi>      defaults to no wrapping module
  converters=<file.yaml> defaults to 'dtsconverters.yaml'
```

## Example

### Step 1: Create `sample.proto`

```protobuf
// sample.proto

package samplePackage;

message SampleMessage {
  optional string sample_field = 1;
}
```

### Step 2: Convert `sample.proto` to binary format DescriptorProtoSet.

```bat
> set Path=%Path%;pathto_protoc-gen-dts
> protoc sample.proto --dts_out=.
```

Results in `sample.d.ts`:

```ts
// Generated with protoc-gen-ts.  DO NOT EDIT!

interface SampleMessage {
    sample_field?: string;
}
```