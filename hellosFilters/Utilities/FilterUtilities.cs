using System;
using System.Collections.Generic;
using System.Linq;

namespace HUIFilters.Utilities
{
    public static class FilterUtilities
    {
        public static IEnumerable<T> EnumerateEnumValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
        public static List<object> EnumerateEnumValuesAsObjectList<T>() => EnumerateEnumValues<T>().Select(x => (object)x).ToList();
    }
}
