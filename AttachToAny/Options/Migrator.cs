using System;
using System.Linq;
using Microsoft.Win32;
using RyanConrad.AttachToAny.Extensions;

namespace RyanConrad.AttachToAny.Options
{
	/// <summary>
	/// This is a "fix" for screw ups...
	/// </summary>
	internal static class Migrator
	{
		/// <summary>
		/// This fixes my mistake for naming IIS process "wp3.exe" and not "w3wp.exe" like it should be.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="descriptorIndex"></param>
		internal static void IISFix(RegistryKey key, int descriptorIndex)
		{
			try
			{
				// get the name.

				var name = key.GetStringValue(ATASettings.Keys.AttachDescriptorName, descriptorIndex);
				var processGroup = key.GetStringValue(ATASettings.Keys.AttachDescriptorProcessNames, descriptorIndex);

				var allProcesses = ((string) key.GetValue(processGroup)).Split(new[] {ATAConstants.ProcessNamesSeparator[0]}, StringSplitOptions.RemoveEmptyEntries);

				const string badProcessName = "wp3.exe";
				// does it have the fouled-up process name?
				var hasWp3 = allProcesses.Any(s => string.Compare(s, badProcessName, StringComparison.OrdinalIgnoreCase) == 0);
				// if it is iis, and it has the wrong process, fix that shit.
				if (string.Compare(name, "iis", StringComparison.OrdinalIgnoreCase) != 0 || !hasWp3)
				{
					return;
				}
				var newList = allProcesses.Where(s => string.Compare(s, badProcessName, StringComparison.OrdinalIgnoreCase) != 0).Concat(new[] {ATAConstants.IIS_PROCESS});
				key.SetValue(processGroup, string.Join(ATAConstants.ProcessNamesSeparator, newList));
			}
			catch (Exception)
			{
				// if we dont have access to write, there is a problem.
			}
		}
	}
}