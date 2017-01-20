using System;
using Microsoft.Win32;

namespace ArcDev.AnotherAttachToAny.Extensions
{
	public static partial class AnotherAttachToAnyExtensions
	{
//	    public static T GetEnumIntValue<T>(this RegistryKey key, string keyFormat, int index)
//	    {
//            var name = string.Format(keyFormat, index);
//            var value =  (int)key.GetValue(name);
//	        return (T)Enum.ToObject(typeof(T), value);
//	    }

        public static T GetEnumValue<T>(this RegistryKey key, string keyFormat, int index, T defaultValue) where T : struct
        {
            var name = string.Format(keyFormat, index);
            var pureValue =  key.GetValue(name);

	        if (pureValue is string)
	        {
	            T rtn;
	            return Enum.TryParse((string) pureValue, true, out rtn) ? rtn : defaultValue;
	        }
            if (pureValue is int)
	        {
                return (T)Enum.ToObject(typeof(T), pureValue);
            }

	        return defaultValue;
	    }

		public static string GetStringValue(this RegistryKey key, string keyFormat, int index)
		{
			var name = string.Format(keyFormat, index);
			return (string) key.GetValue(name);
		}

		public static bool GetBooleanValue(this RegistryKey key, string keyFormat, int index)
		{
			var name = string.Format(keyFormat, index);
			if (key.GetValueKind(name) == RegistryValueKind.DWord)
			{
				var intValue = key.GetValue(name);
				return Convert.ToBoolean(intValue);
			}

			// fallback for older settings
			var str = key.GetStringValue(keyFormat, index);
			bool rtn;
			return bool.TryParse(str, out rtn) && rtn;
		}

		public static void DeleteValue(this RegistryKey key, string keyFormat, int index)
		{
			var name = string.Format(keyFormat, index);
			key.DeleteValue(name, false);
		}

		public static void SetValue(this RegistryKey key, string keyFormat, int index, string value)
		{
			if (value == null)
			{
				return;
			}

			var name = string.Format(keyFormat, index);
			key.SetValue(name, value);
		}

        public static void SetValue(this RegistryKey key, string keyFormat, int index, int value)
		{
			var name = string.Format(keyFormat, index);
			key.SetValue(name, value);
		}

		public static void SetValue(this RegistryKey key, string keyFormat, int index, bool value)
		{
			var name = string.Format(keyFormat, index);
			key.SetValue(name, Convert.ToInt32(value), RegistryValueKind.DWord);
		}
	}
}