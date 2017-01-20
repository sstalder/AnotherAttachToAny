using System.Collections.Generic;
using ArcDev.AnotherAttachToAny.Models;

namespace ArcDev.AnotherAttachToAny.Options
{
    public enum MultiMatchOptions
    {
        // deliberately did not select 0 for an option
        Global = -1,
        All = 2,
        First = 3,
        Last = 4,
        Prompt = 5,
        None = 6,
    }

    // sortof a hack so that the Global Options page doesn't show 'Global' as an option
    public enum MultiMatchOptionsGlobal
    {
        All = MultiMatchOptions.All,
        First = MultiMatchOptions.First,
        Last = MultiMatchOptions.Last,
        Prompt = MultiMatchOptions.Prompt,
        None = MultiMatchOptions.None,
    }


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

            public const string AttachDescriptorMultipleMatchHandling = "AttachDescriptorMultipleMatchHandling{0}";

            public const string AttachDescriptorDefaultMultipleMatchHandling = "AttachDescriptorMultipleMatchHandling";
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