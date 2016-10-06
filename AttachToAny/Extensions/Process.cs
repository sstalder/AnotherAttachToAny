using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using EnvDTE;
using Microsoft.Web.Administration;

namespace RyanConrad.AttachToAny.Extensions
{
	public static partial class AttachToAnyExtensions
	{
		private static readonly Lazy<ServerManager> ServerManagerLazy = new Lazy<ServerManager>(() => new ServerManager());

		public static bool IsIISWorkerProcess(this Process process)
		{
			return process.Name.EndsWith(ATAConstants.IIS_PROCESS, StringComparison.OrdinalIgnoreCase);
		}

		public static string GetAppPoolName(this Process process)
		{
			if (process.IsIISWorkerProcess() == false)
			{
				return null;
			}

			var serverManager = ServerManagerLazy.Value;
			var applicationPoolCollection = serverManager.ApplicationPools;
			var appPool = applicationPoolCollection.FirstOrDefault(ap => ap.WorkerProcesses.Any(wp => wp.ProcessId == process.ProcessID));

			return appPool == null ? null : appPool.Name;
		}

//		public static string GetProcessOwner(this Process process)
//		{
//			var processId = process.ProcessID;
//			var query = "Select * From Win32_Process Where ProcessID = " + processId;
//			var searcher = new ManagementObjectSearcher(query);
//			var processList = searcher.Get();
//
//			foreach (var o in processList)
//			{
//				var obj = (ManagementObject) o;
//				var argList = new[] { string.Empty, string.Empty };
//				var returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
//				if (returnVal == 0)
//				{
//					// return DOMAIN\user
//					return argList[1] + "\\" + argList[0];
//				}
//			}
//
//			return "NO OWNER";
//		}

		//http://stackoverflow.com/questions/777548/how-do-i-determine-the-owner-of-a-process-in-c
		public static string GetProcessUser(this Process proc)
		{
			var process = System.Diagnostics.Process.GetProcessById(proc.ProcessID);

			var processHandle = IntPtr.Zero;
			try
			{
				OpenProcessToken(process.Handle, 8, out processHandle);
				var wi = new WindowsIdentity(processHandle);
				return wi.Name;
//				var user = wi.Name;
//				return user.Contains("\\") ? user.Substring(user.IndexOf("\\") + 1) : user;
			}
			catch
			{
				return null;
			}
			finally
			{
				if (processHandle != IntPtr.Zero)
				{
					CloseHandle(processHandle);
				}
			}
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);
	}
}