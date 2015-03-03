// Generated with protoc-gen-ts.  DO NOT EDIT!

interface SampleMessage {
    sample_field?: string;
    /** TYPE_MESSAGE, TypeName: .samplePackage.ProtoDateTime */
    date_time_field?: string;
}

interface ProtoDateTime {
    /** TYPE_INT32 */
    year: number;
    /** TYPE_INT32 */
    month: number;
    /** TYPE_INT32 */
    day: number;
    /** TYPE_INT32 */
    hour?: number;
    /** TYPE_INT32 */
    minute?: number;
    /** TYPE_INT32 */
    second?: number;
    /** TYPE_INT32 */
    millisecond?: number;
}
