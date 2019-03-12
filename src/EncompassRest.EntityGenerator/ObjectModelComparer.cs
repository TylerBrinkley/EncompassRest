using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EncompassRest
{
    public static class ObjectModelComparer
    {
        public static void Compare(Type rootObjectModelType0, string objectModelName0, Type rootObjectModelType1, string objectModelName1, TextWriter output, Func<string, bool> propertyFilter, Func<Type, BasicType> getBasicType)
        {
            if (rootObjectModelType0 == null)
            {
                throw new ArgumentNullException(nameof(rootObjectModelType0));
            }
            if (rootObjectModelType1 == null)
            {
                throw new ArgumentNullException(nameof(rootObjectModelType1));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (getBasicType == null)
            {
                throw new ArgumentNullException(nameof(getBasicType));
            }

            var visitedTypes = new HashSet<Type>();

            CompareInternal(rootObjectModelType0, rootObjectModelType1);

            void CompareInternal(Type type0, Type type1)
            {
                if (!visitedTypes.Add(type0))
                {
                    return;
                }

                var objectModelType0Properties = type0.GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
                var objectModelType1Properties = type1.GetProperties().ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var pair in objectModelType0Properties)
                {
                    var propertyName = pair.Key;
                    if (propertyFilter?.Invoke(propertyName) != false)
                    {
                        if (objectModelType1Properties.TryGetValue(propertyName, out var type1Property))
                        {
                            objectModelType1Properties.Remove(propertyName);
                            var propertyType0 = pair.Value.PropertyType;
                            var propertyType1 = type1Property.PropertyType;
                            var elementType0 = GetDictionaryValueType(propertyType0);
                            if (elementType0 != null)
                            {
                                var elementType1 = GetDictionaryValueType(propertyType1);
                                if (elementType1 == null || !CompareTypes(elementType0, elementType1))
                                {
                                    OutputTypeDiffers(propertyType0, propertyName, propertyType1, type1Property.Name);
                                }
                            }
                            else if (propertyType0 != typeof(string) && (elementType0 = GetEnumerableElementType(propertyType0)) != null)
                            {
                                var elementType1 = GetEnumerableElementType(propertyType1);
                                if (elementType1 == null || !CompareTypes(elementType0, elementType1))
                                {
                                    OutputTypeDiffers(propertyType0, propertyName, propertyType1, type1Property.Name);
                                }
                            }
                            else if (!CompareTypes(propertyType0, propertyType1))
                            {
                                OutputTypeDiffers(propertyType0, propertyName, propertyType1, type1Property.Name);
                            }
                        }
                        else
                        {
                            output.WriteLine($"{objectModelName1} is missing {type0.Name}.{propertyName} on {type1.Name}");
                        }
                    }
                }

                foreach (var pair in objectModelType1Properties)
                {
                    if (propertyFilter?.Invoke(pair.Key) != false)
                    {
                        output.WriteLine($"{objectModelName0} is missing {type1.Name}.{pair.Key} on {type0.Name}");
                    }
                }

                void OutputTypeDiffers(Type propertyType0, string propertyName0, Type propertyType1, string propertyName1)
                {
                    output.WriteLine($"{type0.Name}.{propertyName0} and {type1.Name}.{propertyName1} types differ: {propertyType0} and {propertyType1}");
                }

                bool CompareTypes(Type t0, Type t1)
                {
                    var basicType0 = getBasicType(t0);
                    var basicType1 = getBasicType(t1);
                    if (basicType0 != basicType1)
                    {
                        return false;
                    }
                    if (basicType0 == BasicType.Object && t0 != typeof(object) && t1 != typeof(object))
                    {
                        if (t0 == typeof(object))
                        {
                            return t1 == typeof(object);
                        }
                        else if (t1 == typeof(object))
                        {
                            return false;
                        }
                        CompareInternal(t0, t1);
                    }
                    return true;
                }
            }
        }

        private static Type GetEnumerableElementType(Type type) => type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GenericTypeArguments[0];

        private static Type GetDictionaryValueType(Type type) => type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))?.GenericTypeArguments[1];
    }

    public enum BasicType
    {
        Object = 0,
        String = 1,
        Numeric = 2,
        Boolean = 3,
        DateTime = 4
    }
}