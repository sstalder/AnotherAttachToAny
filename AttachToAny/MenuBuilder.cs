using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using RyanConrad.AttachToAny.Components;
using RyanConrad.AttachToAny.Extensions;
using RyanConrad.AttachToAny.Models;
using RyanConrad.AttachToAny.Options;
using Process = EnvDTE.Process;

namespace RyanConrad.AttachToAny
{
	internal class MenuBuilder
	{
		private readonly int baseAttachListId = (int) ATAConstants.cmdidAttachToAnyDynamicStart;

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
				throw new ArgumentNullException("attachables");
			}
			var items = OptionsPage.Attachables.Where(f => f.Enabled).ToList();
			for (var i = 0; i < items.Count; ++i)
			{
				var id = baseAttachListId + i;
				AddAttachCommand(mcs, id, x => x.Attachables[i]);
			}
		}

		/// <summary>
		/// Adds the attach command.
		/// </summary>
		/// <param name="mcs">The Menu Command Service.</param>
		/// <param name="commandId"></param>
		/// <param name="getDescriptor">The get descriptor.</param>
		private void AddAttachCommand(IMenuCommandService mcs, int commandId, Func<GeneralOptionsPage, AttachDescriptor> getDescriptor)
		{
			if (mcs == null)
			{
				return;
			}

			var commandIdentifier = new CommandID(ATAGuids.guidAttachToAnyCmdGroup, commandId);
			var existing = mcs.FindCommand(commandIdentifier);
			var descriptor = getDescriptor(OptionsPage);
			if (existing != null)
			{
				((DescriptorMenuCommand) existing).Descriptor = descriptor;
				return;
			}
			var menuItem = new DescriptorMenuCommand(MenuCommandInvokeHandler, commandId, descriptor);
			mcs.AddCommand(menuItem);
		}

		private void MenuCommandInvokeHandler(object s, EventArgs e)
		{
			var menu = (DescriptorMenuCommand) s;
			if (OptionsPage.DTE == null)
			{
				return;
			}
			//var procList = new List<Process>();

			var procList = OptionsPage.DTE.Debugger.LocalProcesses.Cast<Process>().Where(proc => IsMatch(menu.Descriptor, proc)).ToList();

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

			AttachToAnyPackage.ShowProcessManagerDialog(procList);
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