using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RazorLight.Extensions
{
    public static class TypeExtensions
    {
        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            if (anonymousObject is ExpandoObject exp)
            {
                return exp;
            }

            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var propertyDescriptor in anonymousObject.GetType().GetTypeInfo().GetProperties())
            {
                var obj = propertyDescriptor.GetValue(anonymousObject);
                switch (obj?.GetType())
				{
					case Type t when t.IsAnonymousType():
						obj = obj.ToExpando();
						break;

					case Type t when t.IsArray && t.GetElementType().IsAnonymousType():
						var array = (IList) obj;
						var expandoArray = new ExpandoObject[array.Count];
						for (int ix = 0; ix < array.Count; ix++)
						{
							expandoArray[ix] = array[ix].ToExpando();
						}

						obj = expandoArray;
						break;
				}
                expando.Add(propertyDescriptor.Name, obj);
            }

            return (ExpandoObject)expando;
        }

        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetTypeInfo()
                .GetCustomAttributes(typeof(CompilerGeneratedAttribute), false)
                .Any();

            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

    }
}
