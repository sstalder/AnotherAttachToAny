using Microsoft.VisualStudio.Shell.Interop;

namespace RyanConrad.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		public static bool? ReadSettingStringToBoolean(this IVsSettingsReader reader, string key)
		{
			string value;
			reader.ReadSettingString(key, out value);
			if (value == null)
			{
				return null;
			}
			bool rtn;
			return bool.TryParse(value.ToLowerInvariant(), out rtn) && rtn;
		}
	}
}