namespace RequestFlow.Common.Helper
{
    public static class EnumHelper
    {
        public static TEnum ParseSafe<TEnum>(string value, TEnum defaultValue) where TEnum : struct
        {
            return System.Enum.TryParse(value, true, out TEnum result)
                ? result
                : defaultValue;
        }
    }
}
