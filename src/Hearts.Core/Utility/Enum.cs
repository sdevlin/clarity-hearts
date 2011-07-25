using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Hearts.Utility
{
    public static class Enum
    {
        public static IEnumerable<T> GetValues<T>()
        {
            var type = GetEnumType<T>();
            return System.Enum
                .GetValues(type)
                .Cast<T>();
        }

        public static T Parse<T>(string name)
        {
            var type = GetEnumType<T>();
            return (T)System.Enum
                .Parse(type, name);
        }

        public static string GetDescription<T>(this T e)
        {
            var type = GetEnumType<T>();
            return e.GetDescription(type);
        }

        public static string GetDescription(this object e)
        {
            var type = GetEnumType(e);
            return e.GetDescription(type);
        }

        private static string GetDescription(this object e, Type type)
        {
            var info = type.GetField(e.ToString());
            var attrs = info
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>();
            return attrs.Any()
                ? attrs.First().Description
                : e.ToString();
        }

        private static Type GetEnumType<T>()
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("type must be an enum");
            }
            return type;
        }

        private static Type GetEnumType(object e)
        {
            var type = e.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("type must be an enum");
            }
            return type;
        }
    }
}
