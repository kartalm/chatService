using System;
using System.IO;

namespace CrosscuttingConcernCollection.Helper
{
    public static class ObjectExtensionCollection
    {
        public static bool IsNull(this object value)
        {
            return value == null;
        }

        public static bool IsNotNull(this object value)
        {
            return value != null;
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string GetUntil(this string value, string seperator)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            var index = value.IndexOf(seperator, StringComparison.Ordinal);

            return index > 0 ? value.Substring(0, index) : value;
        }

        public static string ToFileNameWithExtension(this string path)
        {
            var fileName = Path.GetFileName(path);

            return fileName;

        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
