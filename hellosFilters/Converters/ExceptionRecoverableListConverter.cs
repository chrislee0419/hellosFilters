using System;
using System.Collections.Generic;
using IPA.Config.Data;
using IPA.Config.Stores;
using IPA.Config.Stores.Converters;

namespace HUIFilters.Converters
{
    public class ExceptionRecoverableListConverter<T> : ListConverter<T>
    {
        public ExceptionRecoverableListConverter() : base()
        { }

        public ExceptionRecoverableListConverter(ValueConverter<T> underlying) : base(underlying)
        { }

        public override List<T> FromValue(Value value, object parent)
        {
            if (!(value is List list))
                throw new ArgumentException("Argument is not a List", nameof(value));

            var collection = Create(list.Count, parent);

            foreach (var item in list)
            {
                try
                {
                    collection.Add(BaseConverter.FromValue(item, parent));
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"Unexpected exception occurred while converting stored config value to {typeof(T).FullName}. Skipping item...");
                    Plugin.Log.Debug(e);
                }
            }

            return collection;
        }
    }

    public class ExceptionRecoverableListConverter<T, TConverter> : ExceptionRecoverableListConverter<T> where TConverter : ValueConverter<T>, new()
    {
        public ExceptionRecoverableListConverter() : base(new TConverter())
        { }
    }
}
