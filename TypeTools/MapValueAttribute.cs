using System;
using System.ComponentModel;

namespace Common.Tools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MapValueAttribute : Attribute
    {
        private object _sqlValue;
        public MapValueAttribute(object sqlValue)
        {
            _sqlValue = sqlValue;
        }

        public string Caption { get; set; }

        public object SqlValue => _sqlValue;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public object AltSqlValue { get; set; }
    }
}
