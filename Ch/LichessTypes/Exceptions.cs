using System;
using System.Collections.Generic;
using System.Text;

namespace Ch.LichessTypes
{

    [Serializable]
    public class LichessConversionException : Exception
    {
        public LichessConversionException() { }
        public LichessConversionException(string message) : base(message) { }
        public LichessConversionException(string message, Exception inner) : base(message, inner) { }
        protected LichessConversionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
