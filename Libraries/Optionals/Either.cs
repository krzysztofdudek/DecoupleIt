using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    public abstract class Either { }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2> : Either
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        [NotNull]
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        [NotNull]
        public Optional<T2> Second { get; }

        public Either(object instance)
        {
            _instance = instance;

            First  = instance is T1 t1 ? (Optional<T1>) t1 : None.Value;
            Second = instance is T2 obj ? (Optional<T2>) obj : None.Value;
        }

        public Either()
        {
            First  = None<T1>.Value;
            Second = None<T2>.Value;
        }

        private readonly object _instance;
    }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2, T3> : Either<T1, T2>
    {
        /// <summary>
        ///     Third optional.
        /// </summary>
        [NotNull]
        public Optional<T3> Third { get; }

        public Either(object instance) : base(instance)
        {
            Third = instance is T3 obj ? (Optional<T3>) obj : None.Value;
        }

        public Either()
        {
            Third = None<T3>.Value;
        }
    }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2, T3, T4> : Either<T1, T2, T3>
    {
        /// <summary>
        ///     Forth optional.
        /// </summary>
        [NotNull]
        public Optional<T4> Fourth { get; }

        public Either(object instance) : base(instance)
        {
            Fourth = instance is T4 obj ? (Optional<T4>) obj : None.Value;
        }

        public Either()
        {
            Fourth = None<T4>.Value;
        }
    }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2, T3, T4, T5> : Either<T1, T2, T3, T4>
    {
        /// <summary>
        ///     Fifth optional.
        /// </summary>
        [NotNull]
        public Optional<T5> Fifth { get; }

        public Either(object instance) : base(instance)
        {
            Fifth = instance is T5 obj ? (Optional<T5>) obj : None.Value;
        }

        public Either()
        {
            Fifth = None<T5>.Value;
        }
    }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2, T3, T4, T5, T6> : Either<T1, T2, T3, T4, T5>
    {
        /// <summary>
        ///     Sixth optional.
        /// </summary>
        [NotNull]
        public Optional<T6> Sixth { get; }

        public Either(object instance) : base(instance)
        {
            Sixth = instance is T6 obj ? (Optional<T6>) obj : None.Value;
        }

        public Either()
        {
            Sixth = None<T6>.Value;
        }
    }

    /// <inheritdoc />
    [PublicAPI]
    public class Either<T1, T2, T3, T4, T5, T6, T7> : Either<T1, T2, T3, T4, T5, T6>
    {
        /// <summary>
        ///     Seventh optional.
        /// </summary>
        [NotNull]
        public Optional<T7> Seventh { get; }

        public Either(object instance) : base(instance)
        {
            Seventh = instance is T7 obj ? (Optional<T7>) obj : None.Value;
        }

        public Either()
        {
            Seventh = None<T7>.Value;
        }
    }
}
