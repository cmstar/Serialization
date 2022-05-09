using System;

namespace cmstar.Serialization.Json.Contracts
{
    /// <summary>
    /// An extension of <see cref="DateTimeOffsetContract"/> that allows to specify
    /// the format for serializing the date.
    /// </summary>
    public class CustomFormatDateTimeOffsetContract : DateTimeOffsetContract
    {
        private string _format;

        /// <summary>
        /// Gets or sets a value which is used to format the date and time.
        /// The format string will be passed to the <see cref="DateTime.ToString()"/>
        /// method during the serializing.
        /// </summary>
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                ArgAssert.NotNull(value, "Format");
                _format = value;
            }
        }

        protected override string ToStringValue(DateTimeOffset value)
        {
            //if _format is null the default format would be used
            return value.ToString(_format);
        }
    }
}
