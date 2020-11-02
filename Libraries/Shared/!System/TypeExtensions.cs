using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class TypeExtensions
    {
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Type> GetAllBaseTypes([NotNull] this Type type)
        {
            var currentBaseType = type.BaseType;

            while (currentBaseType != null)
            {
                yield return currentBaseType;

                currentBaseType = currentBaseType.BaseType;
            }
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Type> GetAllInterfaces([NotNull] this Type type)
        {
            var interfaces = new List<Type>();

            do
            {
                interfaces.AddRange(type.GetInterfaces());
            } while ((type = type.BaseType) != null);

            return interfaces.Distinct()
                             .ToArray();
        }

        public static bool InheritsOrImplements([NotNull] this Type child, [NotNull] Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                ? child.GetGenericTypeDefinition()
                       .AsNotNull()
                : child;

            while (currentChild != typeof(object))
            {
                if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
                    return true;

                currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
                    ? currentChild.BaseType.GetGenericTypeDefinition()
                    : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }

            return false;
        }

        public static bool IsSimple([NotNull] this TypeInfo type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsSimple(type.GetGenericArguments()[0]
                                    .GetTypeInfo()
                                    .AsNotNull());

            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        }

        private static bool HasAnyInterfaces(Type parent, [NotNull] Type child)
        {
            return child.GetInterfaces()
                        .Any(childInterface =>
                        {
                            var currentInterface = childInterface.IsGenericType ? childInterface.GetGenericTypeDefinition() : childInterface;

                            return currentInterface == parent;
                        });
        }

        [NotNull]
        private static Type ResolveGenericTypeDefinition([NotNull] Type parent)
        {
            var shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition()
                               .AsNotNull();

            return parent;
        }
    }
}
