using System.Collections.Generic;
using ArcDev.AnotherAttachToAny.Models;

namespace ArcDev.AnotherAttachToAny.Options
{
	internal static class ATASettings
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
			var items = new List<AttachDescriptor>
			{
				new AttachDescriptor
				{
					Name = "IIS",
					ProcessNames = new[] {ATAConstants.ProcessNames.IISWorkerProcessName}
				},
				new AttachDescriptor
				{
					Name = "IIS Express",
					ProcessNames = new[] {ATAConstants.ProcessNames.IISExpressProcessName}
				},
				new AttachDescriptor
				{
					Name = "NUnit",
					ProcessNames = new[]
					{
						ATAConstants.ProcessNames.NUnitAgent,
						ATAConstants.ProcessNames.NUnit,
						ATAConstants.ProcessNames.NUnitConsole,

						ATAConstants.ProcessNames.NUnitAgentx86,
						ATAConstants.ProcessNames.NUnitx86,
						ATAConstants.ProcessNames.NUnitConsolex86
					}
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