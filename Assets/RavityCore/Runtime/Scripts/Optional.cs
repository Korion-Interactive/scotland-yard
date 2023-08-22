namespace Ravity
{
    public static class Optional
    {
        public static Optional<T> From<T>(T value) => new Optional<T>(value);
    }
    
    public readonly struct Optional<T>
    {
        private readonly T _value;
        public readonly bool HasValue;

        public Optional(T value)
        {
            HasValue = value != null;
            _value = value;
        }

        public T ValueOr(T fallback)
        {
            return HasValue ? _value : fallback;
        }
        
        public bool TryGet(out T value)
        {
            value = HasValue ? _value : default;
            return HasValue;
        }

        public bool TryGetCast<TCast>(out TCast castValue)
        {
            if (HasValue && _value is TCast cast)
            {
                castValue = cast;
                return true;
            }
            castValue = default;
            return false;
        }
    }
}
