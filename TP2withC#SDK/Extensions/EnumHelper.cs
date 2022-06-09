using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TP2withSDK.Entities;
using TP2withSDK.Extensions.Attributes;

namespace TP2withSDK.Extensions
{
    public static class EnumHelper
    {
        public static string GetTextValue<T>(this T enumValue) where T : Enum
        {
            var attribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TextValueAttribute>();
            return attribute?.Value ?? enumValue.ToString();
        }

        public static List<string> GetAllTextValues<T>(int startIndex = 0) where T : Enum
        {
            var enumValues = GetValues<T>();
            return enumValues.Skip(startIndex).Select(enumValue => enumValue.GetTextValue()).ToList();
        }

        private static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}



