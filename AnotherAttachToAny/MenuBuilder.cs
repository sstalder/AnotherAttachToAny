using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
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

			var procList = OptionsPage.Dte.Debugger.LocalProcesses.Cast<Process>().Where(proc => IsMatch(menu.Descriptor, proc)).ToList();

			if (procList.Count == 0)
			{
				return;
			}

			//todo: GlobalAttachTo: logic for GlobalAttachTo

			// Where there is only 1, or "best choice"
			if (procList.Count == 1 || !menu.Descriptor.ChooseProcess)
			{
				procList.First().Attach();
				return;
			}

			AnotherAttachToAnyPackage.ShowProcessManagerDialog(procList);
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