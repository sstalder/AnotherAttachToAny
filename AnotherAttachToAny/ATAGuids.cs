// Guids.cs
// MUST match guids.h
using System;

namespace ArcDev.AnotherAttachToAny {
	internal static class ATAGuids {
		public const string guidAnotherAttachToAnyPkgString = "dedbee85-faec-4043-8d6c-4b71feb0f1ff";

		public const string guidAnotherAttachToAnyCmdGroupString = "8220136f-986d-4736-bed4-be2748321f5e";
		public const string guidAnotherAttachToAnySettingsGroupString = "6e865d89-ecaa-4f6d-9e8e-53d9ab6660b8";

		public static readonly Guid guidAnotherAttachToAnyCmdGroup = new Guid ( guidAnotherAttachToAnyCmdGroupString );
		public static readonly Guid guidAnotherAttachToAnySettingsGroup = new Guid ( guidAnotherAttachToAnySettingsGroupString );
	};
}