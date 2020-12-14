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
            First  = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
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
            First  = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
            Third  = instance is T3 t3 ? (Optional<T3>) t3 : None<T3>.Value;
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
            First  = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
            Third  = instance is T3 t3 ? (Optional<T3>) t3 : None<T3>.Value;
            Fourth = instance is T4 t4 ? (Optional<T4>) t4 : None<T4>.Value;
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
            First  = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
            Third  = instance is T3 t3 ? (Optional<T3>) t3 : None<T3>.Value;
            Fourth = instance is T4 t4 ? (Optional<T4>) t4 : None<T4>.Value;
            Fifth  = instance is T5 t5 ? (Optional<T5>) t5 : None<T5>.Value;
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
            First  = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
            Third  = instance is T3 t3 ? (Optional<T3>) t3 : None<T3>.Value;
            Fourth = instance is T4 t4 ? (Optional<T4>) t4 : None<T4>.Value;
            Fifth  = instance is T5 t5 ? (Optional<T5>) t5 : None<T5>.Value;
            Sixth  = instance is T6 t6 ? (Optional<T6>) t6 : None<T6>.Value;
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
            First   = instance is T1 t1 ? (Optional<T1>) t1 : None<T1>.Value;
            Second  = instance is T2 t2 ? (Optional<T2>) t2 : None<T2>.Value;
            Third   = instance is T3 t3 ? (Optional<T3>) t3 : None<T3>.Value;
            Fourth  = instance is T4 t4 ? (Optional<T4>) t4 : None<T4>.Value;
            Fifth   = instance is T5 t5 ? (Optional<T5>) t5 : None<T5>.Value;
            Sixth   = instance is T6 t6 ? (Optional<T6>) t6 : None<T6>.Value;
            Seventh = instance is T7 t7 ? (Optional<T7>) t7 : None<T7>.Value;
        }
    }
}
