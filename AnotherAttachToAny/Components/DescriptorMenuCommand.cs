using System;
using System.ComponentModel.Design;
using System.Linq;
using ArcDev.AnotherAttachToAny.Models;
using Microsoft.VisualStudio.Shell;

namespace ArcDev.AnotherAttachToAny.Components
{
	internal class DescriptorMenuCommand : OleMenuCommand
	{
		public DescriptorMenuCommand(EventHandler invokeHandler, int commandId, AttachDescriptor descriptor)
			: base(invokeHandler, new CommandID(ATAGuids.guidAnotherAttachToAnyCmdGroup, commandId), descriptor.ToString())
		{
			Descriptor = descriptor;
			BeforeQueryStatus += OnBeforeQueryStatus;
		}

		private void OnBeforeQueryStatus(object s, EventArgs e)
		{
			Visible = Descriptor.Enabled && Descriptor.ProcessNames.Any();
			Text = Descriptor.ToString();
		}

		public AttachDescriptor Descriptor { get; set; }
	}
}