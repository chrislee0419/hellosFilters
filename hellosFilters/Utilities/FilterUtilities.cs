using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using HUIFilters.Filters;

namespace HUIFilters.Utilities
{
    public static class FilterUtilities
    {
        private static StringBuilder _idStringBuilder = new StringBuilder();
        public static string GetIdentifier(this IFilter filter)
        {
            _idStringBuilder.Clear();

            _idStringBuilder.Append(filter.GetType().FullName);
            _idStringBuilder.Append('.');
            _idStringBuilder.Append(filter.Name);

            return _idStringBuilder.ToString();
        }

        public static IEnumerable<T> EnumerateEnumValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
        public static List<object> EnumerateEnumValuesAsObjectList<T>() => EnumerateEnumValues<T>().Select(x => (object)x).ToList();
    }
}
