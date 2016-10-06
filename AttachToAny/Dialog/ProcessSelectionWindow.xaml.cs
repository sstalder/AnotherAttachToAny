using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ArcDev.AttachToAny.Models;
using EnvDTE;

namespace ArcDev.AttachToAny.Dialog
{
	/// <summary>
	/// Interaction logic for ProcessSelectionWindow.xaml
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class ProcessSelectionWindow
	{
		public ProcessSelectionWindow(List<Process> processes)
		{
			Processes = new List<ProcessItem>();
			if (!processes.Any())
			{
				throw new ArgumentException("processes must contain items to show this dialog.");
			}
			foreach (var p in processes)
			{
				Processes.Add(new ProcessItem(p));
			}
			InitializeComponent();
			DataContext = this;
			Title = $"{Title} - {Processes.First().ShortName}";
		}

		public ICollection<ProcessItem> Processes { get; set; }

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ProcessesListView_DoubleClick(object sender, RoutedEventArgs e)
		{
			var lvItem = sender as ListViewItem;
			var item = lvItem?.Content as ProcessItem;
			if (item == null)
			{
				return;
			}
			item.Attach();
			Close();
		}
	}
}