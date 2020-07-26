using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    internal static class TypeExtensions
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
        public static Type[] GetAllInterfaces([NotNull] this Type type)
        {
            var interfaces = new List<Type>();

            do
            {
                interfaces.AddRange(type.GetInterfaces());
            } while ((type = type.BaseType) != null);

            return interfaces.Distinct()
                             .ToArray();
        }

        [CanBeNull]
        internal static LifeTimeAttribute GetTheMostImportantLifetimeAttribute([NotNull] this Type type)
        {
            while (true)
            {
                var attribute = GetLifeTimeAttributeForType(type) ?? GetLifeTimeAttributeForInterfaces(type);

                if (attribute != null)
                    return attribute;

                if (type.BaseType is null)
                    return null;

                type = type.BaseType;
            }
        }

        public static bool InheritsOrImplements([NotNull] this Type child, Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType ? child.GetGenericTypeDefinition() : child;

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

        [CanBeNull]
        private static LifeTimeAttribute GetLifeTimeAttributeForInterfaces([NotNull] Type type)
        {
            var interfaceAttributes = new List<LifeTimeAttribute>();

            foreach (var @interface in type.GetInterfaces())
                interfaceAttributes.AddRange(@interface.GetCustomAttributes(true)
                                                       .OfType<LifeTimeAttribute>()
                                                       .ToList());

            if (interfaceAttributes.Count > 1)
                throw new LifeTimeAttributeAmbiguity($"Type '{type.FullName}' interfaces has multiple lifetime attributes.");

            return interfaceAttributes.Count == 1 ? interfaceAttributes.First() : null;
        }

        [CanBeNull]
        private static LifeTimeAttribute GetLifeTimeAttributeForType([NotNull] Type type)
        {
            var currentTypeAttributes = type.GetCustomAttributes(true)
                                            .OfType<LifeTimeAttribute>()
                                            .ToList();

            if (currentTypeAttributes.Count > 1)
                throw new LifeTimeAttributeAmbiguity($"Type '{type.FullName}' has multiple lifetime attributes.");

            return currentTypeAttributes.Count == 1 ? currentTypeAttributes.First() : null;
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

        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();

            return parent;
        }
    }
}
