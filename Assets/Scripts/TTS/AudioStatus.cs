namespace msc
{
    /**
     * MSPSampleStatus indicates how the sample buffer should be handled
     * MSP_AUDIO_SAMPLE_FIRST		- The sample buffer is the start of audio
     * If recognizer was already recognizing, it will discard
     * audio received to date and re-start the recognition
     * MSP_AUDIO_SAMPLE_CONTINUE	- The sample buffer is continuing audio
     * MSP_AUDIO_SAMPLE_LAST		- The sample buffer is the end of audio
     * The recognizer will cease processing audio and
     * return results
     * Note that sample statii can be combined; for example, for file-based input
     * the entire file can be written with SAMPLE_FIRST | SAMPLE_LAST as the
     * status.
     * Other flags may be added in future to indicate other special audio
     * conditions such as the presence of AGC
     */
    public enum AudioStatus
    {
        MSP_AUDIO_SAMPLE_INIT     = 0x00,
        MSP_AUDIO_SAMPLE_FIRST    = 0x01,
        MSP_AUDIO_SAMPLE_CONTINUE = 0x02,
        MSP_AUDIO_SAMPLE_LAST     = 0x04
    }
}