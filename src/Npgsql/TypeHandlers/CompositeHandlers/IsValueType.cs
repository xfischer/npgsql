namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    static class IsValueType<T>
    {
        public static readonly bool Value = typeof(T).IsValueType;
    }
}
