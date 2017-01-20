using System;
using System.Linq;
using ArcDev.AnotherAttachToAny.Extensions;
using Microsoft.Win32;

namespace ArcDev.AnotherAttachToAny.Options
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
				var newList = allProcesses.Where(s => string.Compare(s, badProcessName, StringComparison.OrdinalIgnoreCase) != 0).Concat(new[] {ATAConstants.ProcessNames.IISWorkerProcessName});
				key.SetValue(processGroup, string.Join(ATAConstants.ProcessNamesSeparator, newList));
			}
			catch (Exception)
			{
				// if we dont have access to write, there is a problem.
			}
		}

	    internal static void ChooseProcessUpdate(RegistryKey key, int descriptorIndex)
	    {
	        try
	        {
	            var oldName = string.Format(ATASettings.Keys.AttachDescriptorChooseProcess, descriptorIndex);
	            var valueNames = key.GetValueNames();
	            if (valueNames.Any(name => name.Equals(oldName, StringComparison.OrdinalIgnoreCase)) == false)
	            {
	                // old value not present so nothing to do
	                return;
	            }

                var value = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorChooseProcess, descriptorIndex);
	            key.DeleteValue(oldName, false); // delete the old key

	            var newValue = value ? MultiMatchOptions.Prompt : MultiMatchOptions.Global;

                key.SetValue(ATASettings.Keys.AttachDescriptorMultipleMatchHandling, descriptorIndex, newValue.ToString());
	        }
            catch (Exception)
            {
                // if we dont have access to write, there is a problem.
            }
        }
	}
}