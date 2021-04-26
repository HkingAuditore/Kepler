using System;
using System.Runtime.Serialization;

namespace XmlSaver
{
    /// <summary>
    /// 存档报错
    /// </summary>
    [Serializable]
    public class SaverException : Exception
    {
        public SaverException()
        {
        }

        public SaverException(string message) : base(message)
        {
        }

        public SaverException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SaverException(
            SerializationInfo info,
            StreamingContext  context) : base(info, context)
        {
        }
    }
}