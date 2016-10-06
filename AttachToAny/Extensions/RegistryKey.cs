using Microsoft.Win32;

namespace RyanConrad.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		public static string GetStringValue(this RegistryKey key, string format, params object[] args)
		{
			return (string) key.GetValue(format.With(args));
		}

		public static bool GetBooleanValue(this RegistryKey key, string format, params object[] args)
		{
			var str = key.GetStringValue(format, args);
			bool rtn;
			return bool.TryParse(str, out rtn) && rtn;
		}
	}
}