namespace DatabaseQueue.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmptyEx(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        public static string FormatEx(this string self, params object[] args)
        {
            return string.Format(self, args);
        }
    }
}
