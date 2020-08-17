namespace rz_frzbn.Singletons.utils
{
    public class Exceptions
    {
        [System.Serializable]
        public class IllegalStateException : System.Exception
        {
            public IllegalStateException() { }
            public IllegalStateException(string message) : base(string.Format("Illegal state {0}", message)) { }
            public IllegalStateException(string message, System.Exception inner) : base(string.Format("Illegal state {0}", message), inner) { }
            protected IllegalStateException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}