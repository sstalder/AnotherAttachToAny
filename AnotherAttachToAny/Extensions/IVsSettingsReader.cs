using Microsoft.VisualStudio.Shell.Interop;

namespace ArcDev.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		public static string ReadSettingString(this IVsSettingsReader reader, string keyFormat, int index)
		{
			var key = string.Format(keyFormat, index);
			string value;
			reader.ReadSettingString(key, out value);
			return value;
		}

		public static bool? ReadSettingStringToBoolean(this IVsSettingsReader reader, string keyFormat, int index)
		{
			var key = string.Format(keyFormat, index);
			string value;
			reader.ReadSettingString(key, out value);
			if (value == null)
			{
				return null;
			}
			bool rtn;
			return bool.TryParse(value.ToLowerInvariant(), out rtn) && rtn;
		}

		public static void WriteSettingString(this IVsSettingsWriter writer, string keyFormat, int index, string value)
		{
			if (value == null)
			{
				return;
			}
			var name = string.Format(keyFormat, index);
			writer.WriteSettingString(name, value);
		}

		public static void WriteSettingString(this IVsSettingsWriter writer, string keyFormat, int index, bool value)
		{
			var strValue = value.ToString().ToLowerInvariant();
			writer.WriteSettingString(keyFormat, index, strValue);
		}
	}
}