namespace msc
{
    /**
     * The enumeration MSPepState contains the current endpointer state
     * MSP_EP_LOOKING_FOR_SPEECH	- Have not yet found the beginning of speech
     * MSP_EP_IN_SPEECH			- Have found the beginning, but not the end of speech
     * MSP_EP_AFTER_SPEECH			- Have found the beginning and end of speech
     * MSP_EP_TIMEOUT				- Have not found any audio till timeout
     * MSP_EP_ERROR				- The endpointer has encountered a serious error
     * MSP_EP_MAX_SPEECH			- Have arrive the max size of speech
     */
    public enum EpStatus
    {
        MSP_EP_LOOKING_FOR_SPEECH = 0,
        MSP_EP_IN_SPEECH          = 1,
        MSP_EP_AFTER_SPEECH       = 3,
        MSP_EP_TIMEOUT            = 4,
        MSP_EP_ERROR              = 5,
        MSP_EP_MAX_SPEECH         = 6,
        MSP_EP_IDLE               = 7 // internal state after stop and before start
    }
}