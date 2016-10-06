﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using RyanConrad.AttachToAny.Components;

namespace RyanConrad.AttachToAny.Models
{
	[DisplayName("Attach Descriptor")]
	public class AttachDescriptor
	{
		public AttachDescriptor()
		{
			 _processNameRegexesLazy = new Lazy<List<Regex>>(ProcessNamesToRegexes);
			 _usernameRegexLazy = new Lazy<Regex>(UsernameToRegex);
			 _appPoolRegexLazy = new Lazy<Regex>(AppPoolToRegex);

			Enabled = true;
			ProcessNames = new List<string>();
			ChooseProcess = false;
			IsProcessNamesRegex = false;
			IsUsernameRegex = false;
		}

		[DisplayName("(Name)")]
		[Category("General")]
		[Description("The name of the command.")]
		public string Name { get; set; }

		[DisplayName("Processes")]
		[Category("General")]
		[Description("A list of process names to look for. It will attach to the first one that is found.")]
		[LocDisplayName("Process")]
		[Editor(typeof(StringListUIEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(StringListTypeConverter))]
		public IEnumerable<string> ProcessNames { get; set; }

		[DisplayName("ProcessNames is Regex List")]
		[Category("General")]
		[Description("Treat the ProcessNames value(s) as (a) regular expression(s).")]
		[DefaultValue(false)]
		public bool IsProcessNamesRegex { get; set; }

		[DisplayName("Enabled")]
		[Category("General")]
		[Description("Enable/Disable this menu item.")]
		[DefaultValue(true)]
		public bool Enabled { get; set; }

		[DisplayName("Choose which Process")]
		[LocDisplayName("Choose which Process")]
		[Category("General")]
		[Description("Where there are multiple instances of a process, show a dialog that will allow you to choose which process to attach to. Setting to false will use a 'best guess' on which process to attach to.")]
		[DefaultValue(false)]
		public bool ChooseProcess { get; set; }

		[DisplayName("Username")]
		[Category("General")]
		[Description("The username regex executing the process.")]
		public string Username { get; set; }

		[DisplayName("Username is Regex")]
		[Category("General")]
		[Description("Treat the Username value as a regular expression.")]
		[DefaultValue(false)]
		public bool IsUsernameRegex { get; set; }

		//todo: support AppPool
		[DisplayName("AppPool")]
		[Category("General")]
		[Description("The AppPool regex executing the process.")]
		public string AppPool { get; set; }

		[DisplayName("AppPool is Regex")]
		[Category("General")]
		[Description("Treat the AppPool value as a regular expression.")]
		[DefaultValue(false)]
		public bool IsAppPoolRegex { get; set; }

		private readonly Lazy<List<Regex>> _processNameRegexesLazy;
		private readonly Lazy<Regex> _usernameRegexLazy;
		private readonly Lazy<Regex> _appPoolRegexLazy;

		private List<Regex> ProcessNamesToRegexes()
		{
			return ProcessNames.Select(name => new Regex(name)).ToList();
		}

		private Regex AppPoolToRegex()
		{
			return new Regex(AppPool);
		}

		private Regex UsernameToRegex()
		{
			return new Regex(Username);
		}

		[Browsable(false)]
		public List<Regex> ProcessNameRegexes
		{
			get { return _processNameRegexesLazy.Value; }
		}

		[Browsable(false)]
		public Regex AppPoolRegex
		{
			get { return _appPoolRegexLazy.Value; }
		}

		[Browsable(false)]
		public Regex UsernameRegex
		{
			get { return _usernameRegexLazy.Value; }
		}

		public override string ToString()
		{
			var text = string.IsNullOrWhiteSpace(Name) ?
				(ProcessNames == null || !ProcessNames.Any() ?
						"[Unused]" :
						string.Join(",", ProcessNames)
				)
				: Name;
			return text;
		}
	}
}