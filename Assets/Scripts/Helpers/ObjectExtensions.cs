﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using Object = System.Object;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod =
            typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly Random ROLLER = new Random();

        public static string CombineToString<T, K>(this Tuple<T, K> tuple)
        {
            return tuple.Item1 + " : " + tuple.Item2;
        }

        public static T GetRandom<T>(this ICollection<T> collection)
        {
            return collection.Count == 0 
                ? default 
                : collection.ElementAt(ROLLER.Next(collection.Count));
        }

        public static string Print(this IEnumerable collection)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var item in collection)
            { 
                builder.AppendLine(item.ToString());
            }
            
            return builder.ToString();
        }
        
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String))
            {
                return true;
            }

            return (type.IsValueType & type.IsPrimitive);
        }

        public static string ToTitleCase(this string data)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(data);
        }

        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            return enumerable == null || enumerable.GetEnumerator().MoveNext() == false;
        }
        
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == null)
            {
                return null;
            }

            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) => accumulator.SelectMany(
                    accseq => sequence,
                    (accseq, item) => accseq.Concat(new[] { item })));
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject,
                new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }

        public static Array GetAllChildren(this Node node)
        {
            Array array = node.GetChildren();
            if (array.Count <= 0)
            {
                return array;
            }
            foreach (var obj in array)
            {
                if (!(obj is Node innerNode))
                {
                    continue;
                }
                Array inner = innerNode.GetAllChildren();

                foreach (var innerObj in inner)
                {
                    array.Add(innerObj);
                }
            }

            return array;
        }

        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null)
            {
                return null;
            }

            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
            {
                return originalObject;
            }

            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }

            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }

            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    System.Array clonedArray = (System.Array) cloneObject;
                    clonedArray.ForEach((array, indices) =>
                        array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }
            }

            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject,
            IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType,
                    BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject,
            Type typeToReflect,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                        BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false)
                {
                    continue;
                }

                if (IsPrimitive(fieldInfo.FieldType))
                {
                    continue;
                }

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        public static T Copy<T>(this T original)
        {
            return (T) Copy((Object) original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }
    }

    public static class IEnumerableExtensions
    {
        public static void ForEach(this System.Array array, Action<System.Array, int[]> action)
        {
            if (array.LongLength > 0)
            {
                return;
            }

            ArrayTraverse walker = new ArrayTraverse(array);
            do
            {
                action(array, walker.Position);
            } while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(System.Array array)
        {
            this.maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                this.maxLengths[i] = array.GetLength(i) - 1;
            }

            this.Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < this.Position.Length; ++i)
            {
                if (this.Position[i] < this.maxLengths[i])
                {
                    this.Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        this.Position[j] = 0;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}