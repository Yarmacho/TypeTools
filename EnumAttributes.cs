using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Common.Tools
{
    public class EnumAttributes
    {
        public static Dictionary<Type, List<KeyValuePair<string, MapValueAttribute>>> _enumCache = new Dictionary<Type, List<KeyValuePair<string, MapValueAttribute>>>();

        private List<KeyValuePair<string, MapValueAttribute>> _enumValues;
        private EnumAttributes(Type enumType)
        {
            if (!_enumCache.TryGetValue(enumType, out List<KeyValuePair<string, MapValueAttribute>> enumValues))
            {
                enumValues = new List<KeyValuePair<string, MapValueAttribute>>();
                foreach (var value in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var mapValue = value.GetCustomAttributes<MapValueAttribute>().FirstOrDefault();
                    if (mapValue != null)
                    {
                        enumValues.Add(new KeyValuePair<string, MapValueAttribute>(value.Name, mapValue));
                    }
                }
                _enumCache.Add(enumType, enumValues);
            }

            _enumValues = enumValues;
        }

        public static T ValueToEnum<T>(object value) where T : Enum
        {
            return (T)ValueToEnum(value, typeof(T));
        }

        internal static object ValueToEnum(object value, Type enumType)
        {
            var attributes = new EnumAttributes(enumType);
            return attributes.valueToEnum(value, enumType);
        }

        private string getEnumField(object value)
        {
            foreach (var enumValue in _enumValues)
            {
                if (enumValue.Key.Equals(value) || enumValue.Value.SqlValue.Equals(value) ||
                    (enumValue.Value.AltSqlValue != null && enumValue.Value.AltSqlValue.Equals(value)))
                {
                    return enumValue.Key;
                }
            }

            throw new InvalidEnumArgumentException($"Enum member {value} is missing");
        }

        private object valueToEnum(object value, Type enumType)
        {
            return Enum.Parse(enumType, getEnumField(value));
        }

        public static object EnumToValue<T>(Enum member)
        {
            return EnumToValue(member, member.GetType());
        }

        public static object EnumToValue(Enum member, Type enumType)
        {
            var attributes = new EnumAttributes(enumType);
            return attributes.enumToValue(member);
        }

        public static string GetEnumCaption(Enum member, Type enumType)
        {
            var attributes = new EnumAttributes(enumType);
            return attributes.getEnumCaption(member);
        }

        public static string GetEnumCaption<T>(Enum member)
        {
            return GetEnumCaption(member, typeof(T));
        }

        private object enumToValue(Enum member)
        {
            var stringValue = member.ToString();
            foreach (var enumValue in _enumValues)
            {
                if (enumValue.Key.Equals(stringValue))
                {
                    return enumValue.Value.SqlValue;
                }
            }

            return stringValue;
        }
        
        private string getEnumCaption(Enum member)
        {
            var stringValue = member.ToString();
            foreach (var enumValue in _enumValues)
            {
                if (enumValue.Key.Equals(stringValue))
                {
                    return string.IsNullOrEmpty(enumValue.Value.Caption)
                        ? enumValue.Value.SqlValue.ToString()
                        : enumValue.Value.Caption;
                }
            }

            return stringValue;
        }
    }
}
