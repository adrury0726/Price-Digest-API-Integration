namespace PriceDigestAPI
{
    public static class PDUtility
    {
        public static string nullToNullString(object obj)
        {
            if (obj == DBNull.Value) return null;
            else return obj.ToString();
        }
        public static bool? nullToNullBool(object obj)
        {
            var result = 0;
            if (obj == DBNull.Value || obj == null) return null;
            else { int.TryParse(obj.ToString(), out result); }
            if (result == 0) return false;
            return true;
        }
        public static double? nullToNullDouble(object obj)
        {
            var result = 0.0;
            if (obj == DBNull.Value || obj == null) return null;
            else { double.TryParse(obj.ToString(), out result); }
            return result;
        }
        public static decimal? nullToNullDecimal(object obj)
        {
            decimal result = 0;
            if (obj == DBNull.Value || obj == null) return null;
            else { decimal.TryParse(obj.ToString(), out result); }
            return result;
        }
        public static int? nullToNullInt(object obj)
        {
            var result = 0;
            if (obj == DBNull.Value || obj == null) return null;
            else { int.TryParse(obj.ToString(), out result); }
            return result;
        }
        public static long? nullToNullLong(object obj)
        {
            var result = (long)0;
            if (obj == DBNull.Value || obj == null) return null;
            else { long.TryParse(obj.ToString(), out result); }
            return result;
        }
        public static byte NullToZeroByte(object o)
        {
            byte result = 0;
            if (o != null)
                byte.TryParse(o.ToString(), out result);
            return result;
        }
        public static int NullToZeroInt(object o)
        {
            var result = 0;
            if (o != DBNull.Value && o != null)
                int.TryParse(o.ToString(), out result);
            return result;
        }
        public static bool NullToFalseBool(object o)
        {
            var result = false;
            if (o != DBNull.Value && o != null) { bool.TryParse(o.ToString(), out result); }
            return result;
        }
        public static double NullToZeroDouble(object o)
        {
            var result = 0.0;
            if (o != null) { double.TryParse(o.ToString(), out result); }
            return result;
        }
        public static decimal NullToZeroDecimal(object o)
        {
            var result = decimal.Zero;
            if (o != null) { decimal.TryParse(o.ToString(), out result); }
            return result;
        }
        public static string NullToString(object o) => o == DBNull.Value || o == null ? "" : o?.ToString() ?? string.Empty;
        public static List<T> emptyList<T>(List<T> list, Type type)
        {
            if (list != null && !list.Any()) return null;
            return list;
        }
        public static KeyValuePair<string, string> dereferenceKey(string key)
        {
            return new KeyValuePair<string, string>(key.Substring(0, key.IndexOf('-') + 1), key.Substring(key.IndexOf('-') + 1));
        }
        public static List<KeyValuePair<string, string>> dereferenceKeys(List<string> keys)
        {
            List<KeyValuePair<string, string>> ret = new List<KeyValuePair<string, string>>();
            foreach (string key in keys) ret.Add(dereferenceKey(key));
            return ret;
        }
    }
}
