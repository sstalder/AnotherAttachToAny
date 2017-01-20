using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using ArcDev.AnotherAttachToAny.Components;
using ArcDev.AnotherAttachToAny.Extensions;
using ArcDev.AnotherAttachToAny.Models;
using ArcDev.AnotherAttachToAny.Options;
using Process = EnvDTE.Process;

namespace ArcDev.AnotherAttachToAny
{
	internal class MenuBuilder
	{
		private const int BaseAttachListId = (int) ATAConstants.cmdidAnotherAttachToAnyDynamicStart;

		public MenuBuilder(GeneralOptionsPage optionsPage)
		{
			OptionsPage = optionsPage;
		}

		private GeneralOptionsPage OptionsPage { get; }

		public void BuildMenuItems(OleMenuCommandService mcs)
		{
			if (OptionsPage.Attachables == null)
			{
				Debug.WriteLine("ATA: No Attachables found.");
				// ReSharper disable once NotResolvedInText
				throw new ArgumentNullException("attachables");
			}
			var items = OptionsPage.Attachables.Where(f => f.Enabled).ToList();
			for (var i = 0; i < items.Count; ++i)
			{
				var id = BaseAttachListId + i;
				var descriptor = OptionsPage.Attachables[i];

				descriptor.Shortcut = (i+1).ToString()[0];

				AddAttachCommand(mcs, id, descriptor);
			}
		}

		private void AddAttachCommand(IMenuCommandService mcs, int commandId, AttachDescriptor descriptor)
		{
			if (mcs == null)
			{
				return;
			}

			var commandIdentifier = new CommandID(ATAGuids.guidAnotherAttachToAnyCmdGroup, commandId);
			var existing = mcs.FindCommand(commandIdentifier);
			if (existing != null)
			{
				((DescriptorMenuCommand)existing).Descriptor = descriptor;
				return;
			}
			var menuItem = new DescriptorMenuCommand(MenuCommandInvokeHandler, commandId, descriptor);
			mcs.AddCommand(menuItem);
		}

		private void MenuCommandInvokeHandler(object s, EventArgs e)
		{
			var menu = (DescriptorMenuCommand) s;
			if (OptionsPage.Dte == null)
			{
				return;
			}

			var procList = OptionsPage.Dte.Debugger.LocalProcesses.Cast<Process>().Where(proc => IsMatch(menu.Descriptor, proc)).OrderBy(p => p.ProcessID).ToList();

			if (procList.Count == 0)
			{
				return;
			}

            // Where there is only 1
            if (procList.Count == 1)
			{
				procList.First().Attach();
				return;
			}

            // multiple matches
            var multiMatchHandling = menu.Descriptor.MultiMatchHandling != MultiMatchOptions.Global ? menu.Descriptor.MultiMatchHandling : (MultiMatchOptions)OptionsPage.MultipleMatchHandlingDefault;
            switch (multiMatchHandling)
		    {
		        case MultiMatchOptions.None:
		            MessageBox.Show("Multiple processes found and option is set to attach to None.", "AnotherAttachToAny");
		            return;
                case MultiMatchOptions.First:
                    procList.First().Attach();
		            return;
                case MultiMatchOptions.Last:
                    procList.Last().Attach();
		            return;
                case MultiMatchOptions.Prompt:
                    AnotherAttachToAnyPackage.ShowProcessManagerDialog(procList);
		            return;
                case MultiMatchOptions.All:
                    procList.ForEach(p => p.Attach());
		            return;
		    }
		}

		private bool IsMatch(AttachDescriptor descriptor, Process process)
		{
			var rtn = IsProcessMatch(descriptor, process)
			          && IsUsernameMatch(descriptor, process)
			          && IsAppPoolMatch(descriptor, process);
			return rtn;
		}

		protected bool IsProcessMatch(AttachDescriptor descriptor, Process process)
		{
			if (descriptor.ProcessNames.Any() == false)
			{
				return true;
			}

			return descriptor.IsProcessNamesRegex
				? descriptor.ProcessNameRegexes.Any(rgx => rgx.IsMatch(process.Name))
				: descriptor.ProcessNames.Any(name => process.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase));
		}

		protected bool IsUsernameMatch(AttachDescriptor descriptor, Process process)
		{
			if (string.IsNullOrEmpty(descriptor.Username))
			{
				return true;
			}

			var procOwner = process.GetProcessUser();
			if (procOwner == null)
			{
				return true;
			}

			return descriptor.IsProcessNamesRegex
				? descriptor.UsernameRegex.IsMatch(procOwner)
				: procOwner.EndsWith(descriptor.Username, StringComparison.OrdinalIgnoreCase);
		}

		protected bool IsAppPoolMatch(AttachDescriptor descriptor, Process process)
		{
			if (string.IsNullOrEmpty(descriptor.AppPool))
			{
				return true;
			}

			if (process.IsIISWorkerProcess() == false)
			{
				// not IIS
				return true;
			}

			var appPoolName = process.GetAppPoolName();
			if (appPoolName == null)
			{
				return false;
			}

			return descriptor.IsAppPoolRegex
				? descriptor.AppPoolRegex.IsMatch(appPoolName)
				: appPoolName.EndsWith(descriptor.AppPool, StringComparison.OrdinalIgnoreCase);
		}
	}
}