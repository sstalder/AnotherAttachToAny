using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using ArcDev.AnotherAttachToAny.Components;
using ArcDev.AnotherAttachToAny.Extensions;
using ArcDev.AnotherAttachToAny.Models;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ArcDev.AnotherAttachToAny.Options
{
	public class GeneralOptionsPage : DialogPage
	{
		public event EventHandler<EventArgs> SettingsLoaded;

		[Editor(typeof(CollectionEditor<AttachDescriptor>), typeof(UITypeEditor))]
		[TypeConverter(typeof(IListTypeConverter))]
		[Category("Another Attach To Any")]
		[LocDisplayName("Attachables")]
		[Description("The items that can be used to attach to processes for debugging.")]
		public ReadOnlyCollection<AttachDescriptor> Attachables { get; set; }

		//[Category ( "Another Attach To Any" )]
		//[LocDisplayName ( "Choose which Process" )]
		//[DisplayName ( "Choose which Process" )]
		//[Description ( "Where there are multiple instances of a process, show a dialog that will allow you to choose which process to attach to. Setting to false will use a 'best guess' on which process to attach to." )]
		//[DefaultValue ( false )]
		//public bool ChooseProcess { get; set; }

		//todo: GlobalAttachTo: {first, last, all, prompt, random, none} First matching process; Last matching process; All matching processes

		protected override void OnApply(PageApplyEventArgs e)
		{
			if (e.ApplyBehavior == ApplyKind.Apply)
			{
				// save the changes
				SaveSettingsToStorage();
				SettingsLoaded?.Invoke(this, EventArgs.Empty);
			}
			base.OnApply(e);
		}

		#region Registry

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void LoadSettingsFromStorage()
		{
			var items = new List<AttachDescriptor>();
			try
			{
				var package = GetServiceSafe<AnotherAttachToAnyPackage>();
				Debug.Assert(package != null, "No package service; we cannot load settings");
				using (var rootKey = package.UserRegistryRoot)
				{
					var path = SettingsRegistryPath;
					var key = rootKey.OpenSubKey(path, true /* writable */);
					if (key != null)
					{
						using (key)
						{
							for (var i = 0; i < ATAConstants.MaxCommands; i++)
							{
								var descriptorName = string.Format(ATASettings.Keys.AttachDescriptorName, i);
								if (key.GetValueNames().Any(name => name.Equals(descriptorName, StringComparison.OrdinalIgnoreCase)))
								{
									Migrator.IISFix(key, i);

									items.Add(new AttachDescriptor
									{
										Name = key.GetStringValue(ATASettings.Keys.AttachDescriptorName, i),
										Enabled = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorEnabled, i),
										ProcessNames = key.GetStringValue(ATASettings.Keys.AttachDescriptorProcessNames, i).Split(new[] {ATAConstants.ProcessNamesSeparator[0]}, StringSplitOptions.RemoveEmptyEntries),
										IsProcessNamesRegex = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex, i),
										ChooseProcess = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorChooseProcess, i),
										Username = key.GetStringValue(ATASettings.Keys.AttachDescriptorUsername, i),
										IsUsernameRegex = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorIsUsernameRegex, i),
										AppPool = key.GetStringValue(ATASettings.Keys.AttachDescriptorAppPool, i),
										IsAppPoolRegex = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i)
									});
								}
								else
								{
									// add an empty one
									items.Add(new AttachDescriptor());
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
			if (items.Count == 0)
			{
				items = ATASettings.DefaultAttachables();
			}

			Attachables = new ReadOnlyCollection<AttachDescriptor>(items);
			OnSettingsLoaded(EventArgs.Empty);
			base.LoadSettingsFromStorage();
		}

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void SaveSettingsToStorage()
		{
			var package = GetServiceSafe<AnotherAttachToAnyPackage>();
			Debug.Assert(package != null, "No package service; we cannot load settings");
			using (var rootKey = package.UserRegistryRoot)
			{
				var path = SettingsRegistryPath;
				var key = rootKey.OpenSubKey(path, true /* writable */) ?? rootKey.CreateSubKey(path);
				if (key == null)
				{
					return;
				}

				using (key)
				{
					for (var i = 0; i < ATAConstants.MaxCommands; i++)
					{
						var item = i >= Attachables.Count ? new AttachDescriptor() : Attachables[i];

						if (string.IsNullOrWhiteSpace(item.Name) && !item.ProcessNames.Any())
						{
							// this should remove "cleared" items
							key.DeleteValue(ATASettings.Keys.AttachDescriptorName, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorEnabled, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorProcessNames, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorChooseProcess, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorUsername, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsUsernameRegex, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorAppPool, i);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i);
						}
						else
						{
							key.SetValue(ATASettings.Keys.AttachDescriptorName, i, item.Name);
							key.SetValue(ATASettings.Keys.AttachDescriptorEnabled, i, item.Enabled);
							key.SetValue(ATASettings.Keys.AttachDescriptorProcessNames, i, string.Join(ATAConstants.ProcessNamesSeparator, item.ProcessNames));
							key.SetValue(ATASettings.Keys.AttachDescriptorChooseProcess, i, item.ChooseProcess);
							key.SetValue(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex, i, item.IsProcessNamesRegex);
							key.SetValue(ATASettings.Keys.AttachDescriptorUsername, i, item.Username);
							key.SetValue(ATASettings.Keys.AttachDescriptorIsUsernameRegex, i, item.IsUsernameRegex);
							key.SetValue(ATASettings.Keys.AttachDescriptorAppPool, i, item.AppPool);
							key.SetValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i, item.IsAppPoolRegex);
						}
					}
				}
			}
			base.SaveSettingsToStorage();
		}

		#endregion Registry

		#region XML

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void LoadSettingsFromXml(IVsSettingsReader reader)
		{
			var items = new List<AttachDescriptor>();
			try
			{
				for (var i = 0; i < ATAConstants.MaxCommands; i++)
				{
					// read from the xml feed
					var item = new AttachDescriptor();
					try
					{
						var value = reader.ReadSettingString(ATASettings.Keys.AttachDescriptorName, i);
						if (value != null)
						{
							item.Name = value;
						}

						var enabled = reader.ReadSettingStringToBoolean(ATASettings.Keys.AttachDescriptorEnabled, i);
						if (enabled.HasValue)
						{
							item.Enabled = enabled.Value;
						}

						var parsedBool = reader.ReadSettingStringToBoolean(ATASettings.Keys.AttachDescriptorChooseProcess, i);
						if (parsedBool.HasValue)
						{
							item.ChooseProcess = parsedBool.Value;
						}

						value = reader.ReadSettingString(ATASettings.Keys.AttachDescriptorProcessNames, i);
						if (value != null)
						{
							item.ProcessNames = value.Split(new[] {ATAConstants.ProcessNamesSeparator[0]}, StringSplitOptions.RemoveEmptyEntries);
						}

						parsedBool = reader.ReadSettingStringToBoolean(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex, i);
						if (parsedBool.HasValue)
						{
							item.IsProcessNamesRegex = parsedBool.Value;
						}

						value = reader.ReadSettingString(ATASettings.Keys.AttachDescriptorUsername, i);
						if (value != null)
						{
							item.Username = value;
						}

						parsedBool = reader.ReadSettingStringToBoolean(ATASettings.Keys.AttachDescriptorIsUsernameRegex, i);
						if (parsedBool.HasValue)
						{
							item.IsUsernameRegex = parsedBool.Value;
						}

						value = reader.ReadSettingString(ATASettings.Keys.AttachDescriptorAppPool, i);
						if (value != null)
						{
							item.AppPool = value;
						}

						parsedBool = reader.ReadSettingStringToBoolean(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i);
						if (parsedBool.HasValue)
						{
							item.IsAppPoolRegex = parsedBool.Value;
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Unhandled exception in LoadSettingsFromXml:{ex}");
					}

					if (!string.IsNullOrWhiteSpace(item.Name) && item.ProcessNames != null && item.ProcessNames.Any())
					{
						items.Add(item);
					}
					else
					{
						// this ensures it is a clean item if any of the other properties were saved previously.
						items.Add(new AttachDescriptor());
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unhandled exception in LoadSettingsFromXml:{ex}");
			}
			Attachables = new ReadOnlyCollection<AttachDescriptor>(items);
			// notify of newly loaded settings
			OnSettingsLoaded(EventArgs.Empty);
			base.LoadSettingsFromXml(reader);
		}

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void SaveSettingsToXml(IVsSettingsWriter writer)
		{
			for (var i = 0; i < ATAConstants.MaxCommands; i++)
			{
				var item = i >= Attachables.Count ? new AttachDescriptor() : Attachables[i];

				if (string.IsNullOrWhiteSpace(item.Name) || !item.ProcessNames.Any())
				{
					continue;
				}
				// only items with names and processes
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorName, i, item.Name);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorEnabled, i, item.Enabled);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorProcessNames, i, string.Join(ATAConstants.ProcessNamesSeparator, item.ProcessNames));
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorChooseProcess, i, item.ChooseProcess);

				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex, i, item.IsProcessNamesRegex);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorUsername, i, item.Username);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsUsernameRegex, i, item.IsUsernameRegex);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorAppPool, i, item.AppPool);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i, item.IsAppPoolRegex);
			}

			base.SaveSettingsToXml(writer);
		}

		#endregion XML

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public DTE Dte => GetService(typeof(DTE)) as DTE;

		private void OnSettingsLoaded(EventArgs args)
		{
			SettingsLoaded?.Invoke(this, args);
		}

		private T GetServiceSafe<T>() where T : Package
		{
			try
			{
				return (T) GetService(typeof(T));
			}
			catch (Exception)
			{
				return default(T);
			}
		}
	}
}