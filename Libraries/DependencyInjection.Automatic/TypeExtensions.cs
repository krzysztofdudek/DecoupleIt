using System;
using System.Linq;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    internal static class TypeExtensions
    {
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

        [CanBeNull]
        private static LifeTimeAttribute GetLifeTimeAttributeForInterfaces([NotNull] Type type)
        {
            var interfaceAttributes = type.GetInterfaces()
                                          .SelectMany(@interface => @interface.GetCustomAttributes(true)
                                                                              .OfType<LifeTimeAttribute>())
                                          .ToList();

            if (interfaceAttributes.Select(x => x.GetType())
                                   .Distinct()
                                   .Count() > 1)
                throw new LifeTimeAttributeAmbiguity($"Type '{type.FullName}' interfaces has multiple lifetime attributes.");

            return interfaceAttributes.Count > 0 ? interfaceAttributes.First() : null;
        }

        [CanBeNull]
        private static LifeTimeAttribute GetLifeTimeAttributeForType([NotNull] Type type)
        {
            var typeAttributes = type.GetCustomAttributes(true)
                                     .OfType<LifeTimeAttribute>()
                                     .ToList();

            if (typeAttributes.Select(x => x.GetType())
                              .Distinct()
                              .Count() > 1)
                throw new LifeTimeAttributeAmbiguity($"Type '{type.FullName}' has multiple lifetime attributes.");

            return typeAttributes.Count > 0 ? typeAttributes.First() : null;
        }
    }
}
