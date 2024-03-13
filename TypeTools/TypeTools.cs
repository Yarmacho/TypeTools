using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common.Tools
{
    public static class TypeTools
    {
        private static Dictionary<Type, List<MethodInfo>> _operatorsCache = new Dictionary<Type, List<MethodInfo>>();

        public static T Convert<T>(object value)
        {
            return (T)convert(value, typeof(T));
        }

        private static object convert(object value, Type targetType)
        {
            if (value is null || value == DBNull.Value)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : default;
            }

            var valueType = value.GetType();
            if (valueType == targetType || valueType.IsInstanceOfType(targetType))
            {
                return value;
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType.IsGenericType)
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    return Activator.CreateInstance(targetType, convert(value, underlyingType));
                }
            }

            if (valueType.IsEnum)
            {
                return EnumAttributes.EnumToValue((Enum)value, targetType);
            }

            if (targetType.IsEnum)
            {
                return EnumAttributes.ValueToEnum(value, targetType);
            }

            try
            {
                return System.Convert.ChangeType(value, targetType);
            }
            catch
            {
                if (!_operatorsCache.TryGetValue(targetType, out List<MethodInfo> operators))
                {
                    operators = new List<MethodInfo>();
                    foreach (var method in targetType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (method.Name.StartsWith("op_Explicit") || method.Name.StartsWith("op_Implicit"))
                        {
                            operators.Add(method);
                        }
                    }
                    _operatorsCache.Add(targetType, operators);
                }

                foreach (var oper in operators)
                {
                    if (oper.ReturnType == targetType)
                    {
                        return oper.Invoke(null, new object[] { value });
                    }
                }
            }

            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
