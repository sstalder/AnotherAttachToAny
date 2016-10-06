using System.IO;
using ArcDev.AttachToAny.Extensions;
using EnvDTE;

namespace ArcDev.AttachToAny.Models
{
	public class ProcessItem
	{
		public ProcessItem(Process baseProcess)
		{
			BaseProcess = baseProcess;
		}

		public string Name => BaseProcess.Name;

		public string ShortName => Path.GetFileName(Name);

		public string Title => System.Diagnostics.Process.GetProcessById(Id).MainWindowTitle;

		public string DisplayText => GetDisplayText();

		public int Id => BaseProcess.ProcessID;

		public void Attach()
		{
			BaseProcess.Attach();
		}


		public Process BaseProcess { get; }

		private string GetDisplayText()
		{
			return string.IsNullOrWhiteSpace(Title) ? GetShortNameFormatted() : Title;
		}

		private string GetShortNameFormatted()
		{
			if (BaseProcess.IsIISWorkerProcess() == false)
			{
				return ShortName;
			}

			var appPoolName = BaseProcess.GetAppPoolName();

			return appPoolName == null ? ShortName : $"{ShortName} [{appPoolName}]";
		}
	}
}