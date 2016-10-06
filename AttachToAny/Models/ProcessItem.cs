using System.IO;
using EnvDTE;
using RyanConrad.AttachToAny.Extensions;

namespace RyanConrad.AttachToAny.Models
{
	public class ProcessItem
	{
		public ProcessItem(Process baseProcess)
		{
			BaseProcess = baseProcess;
		}

		public string Name
		{
			get { return BaseProcess.Name; }
		}

		public string ShortName
		{
			get { return Path.GetFileName(Name); }
		}

		public string Title
		{
			get { return System.Diagnostics.Process.GetProcessById(Id).MainWindowTitle; }
		}

		public string DisplayText
		{
			get { return GetDisplayText(); }
		}

		public int Id
		{
			get { return BaseProcess.ProcessID; }
		}

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

			return appPoolName == null ? ShortName : "{0} [{1}]".With(ShortName, appPoolName);
		}
	}
}