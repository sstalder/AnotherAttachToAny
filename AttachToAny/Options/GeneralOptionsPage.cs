using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RyanConrad.AttachToAny.Components;
using RyanConrad.AttachToAny.Extensions;
using RyanConrad.AttachToAny.Models;

namespace RyanConrad.AttachToAny.Options
{
	public class GeneralOptionsPage : DialogPage
	{
		public event EventHandler<EventArgs> SettingsLoaded;

		[Editor(typeof(CollectionEditor<AttachDescriptor>), typeof(UITypeEditor))]
		[TypeConverter(typeof(IListTypeConverter))]
		[Category("Attach To Any")]
		[LocDisplayName("Attachables")]
		[Description("The items that can be used to attach to processes for debugging.")]
		public ReadOnlyCollection<AttachDescriptor> Attachables { get; set; }

		//[Category ( "Attach To Any" )]
		//[LocDisplayName ( "Choose which Process" )]
		//[DisplayName ( "Choose which Process" )]
		//[Description ( "Where there are multiple instances of a process, show a dialog that will allow you to choose which process to attach to. Setting to false will use a 'best guess' on which process to attach to." )]
		//[DefaultValue ( false )]
		//public bool ChooseProcess { get; set; }

		//todo: GlobalAttachTo: {first, last, all} First matching process; Last matching process; All matching processes

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

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void LoadSettingsFromStorage()
		{
			var items = new List<AttachDescriptor>();
			try
			{
				var package = GetServiceSafe<AttachToAnyPackage>();
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
								if (key.GetValueNames().Any(x => x.Equals(ATASettings.Keys.AttachDescriptorName.With(i))))
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
										          IsAppPoolRegex = key.GetBooleanValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex, i),
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
		public override void LoadSettingsFromXml(IVsSettingsReader reader)
		{
			var items = new List<AttachDescriptor>();
			try
			{
				for (var i = 0; i < ATAConstants.MaxCommands; i++)
				{
					var nameKey = ATASettings.Keys.AttachDescriptorName.With(i);
					var enabledKey = ATASettings.Keys.AttachDescriptorEnabled.With(i);
					var processesKey = ATASettings.Keys.AttachDescriptorProcessNames.With(i);
					var isProcessesRegexKey = ATASettings.Keys.AttachDescriptorIsProcessNamesRegex.With(i);
					var chooseKey = ATASettings.Keys.AttachDescriptorChooseProcess.With(i);
					var usernameKey = ATASettings.Keys.AttachDescriptorUsername.With(i);
					var isUsernameRegexKey = ATASettings.Keys.AttachDescriptorIsUsernameRegex.With(i);
					var AppPoolKey = ATASettings.Keys.AttachDescriptorAppPool.With(i);
					var isAppPoolRegexKey = ATASettings.Keys.AttachDescriptorIsAppPoolRegex.With(i);

					// read from the xml feed
					var item = new AttachDescriptor();
					try
					{
						string value;
						reader.ReadSettingString(nameKey, out value);
						if (value != null)
						{
							item.Name = value;
						}

						var enabled = reader.ReadSettingStringToBoolean(enabledKey);
						if (enabled.HasValue)
						{
							item.Enabled = enabled.Value;
						}

						var parsedBool = reader.ReadSettingStringToBoolean(chooseKey);
						if (parsedBool.HasValue)
						{
							item.ChooseProcess = parsedBool.Value;
						}

						reader.ReadSettingString(processesKey, out value);
						if (value != null)
						{
							item.ProcessNames = value.Split(new[] {ATAConstants.ProcessNamesSeparator[0]}, StringSplitOptions.RemoveEmptyEntries);
						}

						parsedBool = reader.ReadSettingStringToBoolean(isProcessesRegexKey);
						if (parsedBool.HasValue)
						{
							item.IsProcessNamesRegex = parsedBool.Value;
						}

						reader.ReadSettingString(usernameKey, out value);
						if (value != null)
						{
							item.Username = value;
						}

						parsedBool = reader.ReadSettingStringToBoolean(isUsernameRegexKey);
						if (parsedBool.HasValue)
						{
							item.IsUsernameRegex = parsedBool.Value;
						}

						reader.ReadSettingString(AppPoolKey, out value);
						if (value != null)
						{
							item.AppPool = value;
						}

						parsedBool = reader.ReadSettingStringToBoolean(isAppPoolRegexKey);
						if (parsedBool.HasValue)
						{
							item.IsAppPoolRegex = parsedBool.Value;
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Unhandled exception in LoadSettingsFromXml:{0}", ex);
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
				Debug.WriteLine("Unhandled exception in LoadSettingsFromXml:{0}", ex);
			}
			Attachables = new ReadOnlyCollection<AttachDescriptor>(items);
			// notify of newly loaded settings
			OnSettingsLoaded(EventArgs.Empty);
			base.LoadSettingsFromXml(reader);
		}

		// based on information from : https://github.com/hesam/SketchSharp/blob/master/SpecSharp/SpecSharp/Microsoft.VisualStudio.Shell/DialogPage.cs
		public override void SaveSettingsToStorage()
		{
			var package = GetServiceSafe<AttachToAnyPackage>();
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
							key.DeleteValue(ATASettings.Keys.AttachDescriptorName.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorEnabled.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorProcessNames.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorChooseProcess.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorUsername.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsUsernameRegex.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorAppPool.With(i), false);
							key.DeleteValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex.With(i), false);
						}
						else
						{
							key.SetValue(ATASettings.Keys.AttachDescriptorName.With(i), item.Name);
							key.SetValue(ATASettings.Keys.AttachDescriptorEnabled.With(i), item.Enabled.ToString().ToLowerInvariant());
							key.SetValue(ATASettings.Keys.AttachDescriptorProcessNames.With(i), string.Join(ATAConstants.ProcessNamesSeparator, item.ProcessNames));
							key.SetValue(ATASettings.Keys.AttachDescriptorChooseProcess.With(i), item.ChooseProcess.ToString().ToLowerInvariant());
							key.SetValue(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex.With(i), item.IsProcessNamesRegex.ToString().ToLowerInvariant());
							key.SetValue(ATASettings.Keys.AttachDescriptorUsername.With(i), item.Username ?? string.Empty);
							key.SetValue(ATASettings.Keys.AttachDescriptorIsUsernameRegex.With(i), item.IsUsernameRegex.ToString().ToLowerInvariant());
							key.SetValue(ATASettings.Keys.AttachDescriptorAppPool.With(i), item.AppPool ?? string.Empty);
							key.SetValue(ATASettings.Keys.AttachDescriptorIsAppPoolRegex.With(i), item.IsAppPoolRegex.ToString().ToLowerInvariant());
						}
					}
				}
			}
			base.SaveSettingsToStorage();
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
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorName.With(i), item.Name);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorEnabled.With(i), item.Enabled.ToString().ToLowerInvariant());
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorProcessNames.With(i), string.Join(ATAConstants.ProcessNamesSeparator, item.ProcessNames));
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorChooseProcess.With(i), item.ChooseProcess.ToString().ToLowerInvariant());

				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsProcessNamesRegex.With(i), item.IsProcessNamesRegex.ToString().ToLowerInvariant());
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorUsername.With(i), item.Username);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsUsernameRegex.With(i), item.IsUsernameRegex.ToString().ToLowerInvariant());
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorAppPool.With(i), item.AppPool);
				writer.WriteSettingString(ATASettings.Keys.AttachDescriptorIsAppPoolRegex.With(i), item.IsAppPoolRegex.ToString().ToLowerInvariant());
			}

			base.SaveSettingsToXml(writer);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public DTE DTE
		{
			get { return GetService(typeof(DTE)) as DTE; }
		}

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