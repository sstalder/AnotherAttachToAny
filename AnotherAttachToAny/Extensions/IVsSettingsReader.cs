using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace ArcDev.AnotherAttachToAny.Extensions
{
	public static partial class AnotherAttachToAnyExtensions
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
			var name = string.Format(keyFormat, index);
			int intValue;
			var readResult = reader.ReadSettingBoolean(name, out intValue);
			if (readResult == Microsoft.VisualStudio.VSConstants.S_OK)
			{
				return Convert.ToBoolean(intValue);
			}

			// fallback for older settings
			var strValue = reader.ReadSettingString(keyFormat, index);
			if (strValue == null)
			{
				return null;
			}
			bool rtn;
			return bool.TryParse(strValue.ToLowerInvariant(), out rtn) && rtn;
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
			var name = string.Format(keyFormat, index);
			writer.WriteSettingBoolean(name, Convert.ToInt32(value));
		}
	}
}