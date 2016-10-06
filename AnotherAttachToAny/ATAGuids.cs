// Guids.cs
// MUST match guids.h
using System;

namespace ArcDev.AttachToAny {
	internal static class ATAGuids {
		public const string guidAttachToAnyPkgString = "dedbee85-faec-4043-8d6c-4b71feb0f1ff";

		public const string guidAttachToAnyCmdGroupString = "8220136f-986d-4736-bed4-be2748321f5e";
		public const string guidAttachToAnySettingsGroupString = "6e865d89-ecaa-4f6d-9e8e-53d9ab6660b8";

		public static readonly Guid guidAttachToAnyCmdGroup = new Guid ( guidAttachToAnyCmdGroupString );
		public static readonly Guid guidAttachToAnySettingsGroup = new Guid ( guidAttachToAnySettingsGroupString );
	};
}