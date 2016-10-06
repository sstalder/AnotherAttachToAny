using System.Collections.Generic;
using RyanConrad.AttachToAny.Models;

namespace RyanConrad.AttachToAny.Options
{
	static class ATASettings
	{
		public static class Keys
		{
			public const string AttachDescriptorName = "AttachDescriptorName{0}";
			public const string AttachDescriptorProcessNames = "AttachDescriptorProcessNames{0}";
			public const string AttachDescriptorIsProcessNamesRegex = "AttachDescriptorIsProcessNamesRegex{0}";
			public const string AttachDescriptorEnabled = "AttachDescriptorEnabled{0}";
			public const string AttachDescriptorChooseProcess = "AttachDescriptorChooseProcess{0}";
			public const string AttachDescriptorUsername = "AttachDescriptorUsername{0}";
			public const string AttachDescriptorIsUsernameRegex = "AttachDescriptorIsUsernameRegex{0}";
			public const string AttachDescriptorAppPool = "AttachDescriptorAppPool{0}";
			public const string AttachDescriptorIsAppPoolRegex = "AttachDescriptorIsAppPoolRegex{0}";
		}

		public static List<AttachDescriptor> DefaultAttachables()
		{
			var items = new List<AttachDescriptor>()
			            {
				            new AttachDescriptor
				            {
					            Name = "IIS",
					            ProcessNames = new[] {ATAConstants.IIS_PROCESS},
				            },
				            new AttachDescriptor
				            {
					            Name = "IIS Express",
					            ProcessNames = new[] {"iisexpress.exe"},
				            },
				            new AttachDescriptor
				            {
					            Name = "NUnit",
					            ProcessNames = new[] {"nunit-agent.exe", "nunit.exe", "nunit-console.exe", "nunit-agent-x86.exe", "nunit-x86.exe", "nunit-console-x86.exe"},
				            }
			            };
			var start = items.Count;
			for (var i = start; i < ATAConstants.MaxCommands; ++i)
			{
				items.Add(new AttachDescriptor());
			}
			return items;
		}
	}
}