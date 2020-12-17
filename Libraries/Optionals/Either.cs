using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        public Either(object instance)
        {
            First  = instance is T1 t1 ? t1 : new Optional<T1>();
            Second = instance is T2 t2 ? t2 : new Optional<T2>();
        }
    }

    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2, T3>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        /// <summary>
        ///     Third optional.
        /// </summary>
        public Optional<T3> Third { get; }

        public Either(object instance)
        {
            First  = instance is T1 t1 ? t1 : new Optional<T1>();
            Second = instance is T2 t2 ? t2 : new Optional<T2>();
            Third  = instance is T3 t3 ? t3 : new Optional<T3>();
        }
    }

    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2, T3, T4>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        /// <summary>
        ///     Third optional.
        /// </summary>
        public Optional<T3> Third { get; }

        /// <summary>
        ///     Forth optional.
        /// </summary>
        public Optional<T4> Fourth { get; }

        public Either(object instance)
        {
            First  = instance is T1 t1 ? t1 : new Optional<T1>();
            Second = instance is T2 t2 ? t2 : new Optional<T2>();
            Third  = instance is T3 t3 ? t3 : new Optional<T3>();
            Fourth = instance is T4 t4 ? t4 : new Optional<T4>();
        }
    }

    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2, T3, T4, T5>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        /// <summary>
        ///     Third optional.
        /// </summary>
        public Optional<T3> Third { get; }

        /// <summary>
        ///     Forth optional.
        /// </summary>
        public Optional<T4> Fourth { get; }

        /// <summary>
        ///     Fifth optional.
        /// </summary>
        public Optional<T5> Fifth { get; }

        public Either(object instance)
        {
            First  = instance is T1 t1 ? t1 : new Optional<T1>();
            Second = instance is T2 t2 ? t2 : new Optional<T2>();
            Third  = instance is T3 t3 ? t3 : new Optional<T3>();
            Fourth = instance is T4 t4 ? t4 : new Optional<T4>();
            Fifth  = instance is T5 t5 ? t5 : new Optional<T5>();
        }
    }

    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2, T3, T4, T5, T6>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        /// <summary>
        ///     Third optional.
        /// </summary>
        public Optional<T3> Third { get; }

        /// <summary>
        ///     Forth optional.
        /// </summary>
        public Optional<T4> Fourth { get; }

        /// <summary>
        ///     Fifth optional.
        /// </summary>
        public Optional<T5> Fifth { get; }

        /// <summary>
        ///     Sixth optional.
        /// </summary>
        public Optional<T6> Sixth { get; }

        public Either(object instance)
        {
            First  = instance is T1 t1 ? t1 : new Optional<T1>();
            Second = instance is T2 t2 ? t2 : new Optional<T2>();
            Third  = instance is T3 t3 ? t3 : new Optional<T3>();
            Fourth = instance is T4 t4 ? t4 : new Optional<T4>();
            Fifth  = instance is T5 t5 ? t5 : new Optional<T5>();
            Sixth  = instance is T6 t6 ? t6 : new Optional<T6>();
        }
    }

    /// <summary>
    ///     Implementation of many object types passed as unit.
    /// </summary>
    [PublicAPI]
    public readonly struct Either<T1, T2, T3, T4, T5, T6, T7>
    {
        /// <summary>
        ///     First optional.
        /// </summary>
        public Optional<T1> First { get; }

        /// <summary>
        ///     Second optional.
        /// </summary>
        public Optional<T2> Second { get; }

        /// <summary>
        ///     Third optional.
        /// </summary>
        public Optional<T3> Third { get; }

        /// <summary>
        ///     Forth optional.
        /// </summary>
        public Optional<T4> Fourth { get; }

        /// <summary>
        ///     Fifth optional.
        /// </summary>
        public Optional<T5> Fifth { get; }

        /// <summary>
        ///     Sixth optional.
        /// </summary>
        public Optional<T6> Sixth { get; }

        /// <summary>
        ///     Seventh optional.
        /// </summary>
        public Optional<T7> Seventh { get; }

        public Either(object instance)
        {
            First   = instance is T1 t1 ? t1 : new Optional<T1>();
            Second  = instance is T2 t2 ? t2 : new Optional<T2>();
            Third   = instance is T3 t3 ? t3 : new Optional<T3>();
            Fourth  = instance is T4 t4 ? t4 : new Optional<T4>();
            Fifth   = instance is T5 t5 ? t5 : new Optional<T5>();
            Sixth   = instance is T6 t6 ? t6 : new Optional<T6>();
            Seventh = instance is T7 t7 ? t7 : new Optional<T7>();
        }
    }
}
