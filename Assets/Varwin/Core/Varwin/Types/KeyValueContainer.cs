namespace Varwin
{
    public class KeyValueContainer<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
            
        public KeyValueContainer(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}