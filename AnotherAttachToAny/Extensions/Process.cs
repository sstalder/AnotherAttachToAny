using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using EnvDTE;
using Microsoft.Web.Administration;

namespace ArcDev.AnotherAttachToAny.Extensions
{
	public static partial class AnotherAttachToAnyExtensions
	{
		private static readonly Lazy<ServerManager> ServerManagerLazy = new Lazy<ServerManager>(() => new ServerManager());

		public static bool IsIISWorkerProcess(this Process process)
		{
			return process.Name.EndsWith(ATAConstants.ProcessNames.IISWorkerProcessName, StringComparison.OrdinalIgnoreCase);
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

			return appPool?.Name;
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

		#region from http://stackoverflow.com/questions/777548/how-do-i-determine-the-owner-of-a-process-in-c

		public static string GetProcessUser(this Process proc)
		{
			var process = System.Diagnostics.Process.GetProcessById(proc.ProcessID);

			var processHandle = IntPtr.Zero;
			try
			{
				OpenProcessToken(process.Handle, 8, out processHandle);
				var wi = new WindowsIdentity(processHandle);
				return wi.Name;
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

		/// <summary>
		/// The OpenProcessToken function opens the access token associated with a process.
		/// </summary>
		/// <param name="processHandle">A handle to the process whose access token is opened. The process must have the PROCESS_QUERY_INFORMATION access permission.</param>
		/// <param name="desiredAccess">Specifies an access mask that specifies the requested types of access to the access token. These requested access types are compared with the discretionary access control list (DACL) of the token to determine which accesses are granted or denied.
		///For a list of access rights for access tokens, see Access Rights for Access-Token Objects.</param>
		/// <param name="tokenHandle">A pointer to a handle that identifies the newly opened access token when the function returns.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		///If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
		/// <remarks>Close the access token handle returned through the TokenHandle parameter by calling <see cref="CloseHandle"/>.</remarks>
		/// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379295(v=vs.85).aspx">https://msdn.microsoft.com/en-us/library/windows/desktop/aa379295(v=vs.85).aspx</a>
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

		/// <summary>
		/// Closes an open object handle.
		/// </summary>
		/// <param name="hObject">A valid handle to an open object.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		///If the function fails, the return value is zero. To get extended error information, call GetLastError.
		///If the application is running under a debugger, the function will throw an exception if it receives either a handle value that is not valid or a pseudo-handle value. This can happen if you close a handle twice, or if you call CloseHandle on a handle returned by the FindFirstFile function instead of calling the FindClose function.
		///</returns>
		/// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211(v=vs.85).aspx">https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211(v=vs.85).aspx</a>
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);
		#endregion
	}
}