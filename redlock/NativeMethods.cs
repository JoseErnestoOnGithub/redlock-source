using System;
using System.Runtime.InteropServices;

namespace redlock
{
	// Token: 0x0200000C RID: 12
	internal static class NativeMethods
	{
		// Token: 0x0600003B RID: 59
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegLoadKeyW", SetLastError = true)]
		internal static extern int RegLoadKey(uint hKey, [MarshalAs(UnmanagedType.LPWStr)] [Optional] string lpSubKey, [MarshalAs(UnmanagedType.LPWStr)] string lpFile);

		// Token: 0x0600003C RID: 60
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegUnLoadKeyW", SetLastError = true)]
		internal static extern int RegUnLoadKey(uint hKey, [MarshalAs(UnmanagedType.LPWStr)] [Optional] string lpSubKey);

		// Token: 0x0600003D RID: 61
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryExW", SetLastError = true)]
		internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

		// Token: 0x0600003E RID: 62
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindResourceExW", SetLastError = true)]
		internal static extern IntPtr FindResourceEx(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, ushort wLanguage);

		// Token: 0x0600003F RID: 63
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindResourceExW", SetLastError = true)]
		internal static extern IntPtr FindResourceEx(IntPtr hModule, string lpszType, IntPtr lpszName, ushort wLanguage);

		// Token: 0x06000040 RID: 64
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int SizeofResource(IntPtr hInstance, IntPtr hResInfo);

		// Token: 0x06000041 RID: 65
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResData);

		// Token: 0x06000042 RID: 66
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		// Token: 0x06000043 RID: 67
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "BeginUpdateResourceW", ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

		// Token: 0x06000044 RID: 68
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "UpdateResourceW", ExactSpelling = true, SetLastError = true)]
		internal static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

		// Token: 0x06000045 RID: 69
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "UpdateResourceW", ExactSpelling = true, SetLastError = true)]
		internal static extern bool UpdateResource(IntPtr hUpdate, string lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

		// Token: 0x06000046 RID: 70
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "EndUpdateResourceW", ExactSpelling = true, SetLastError = true)]
		internal static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

		// Token: 0x06000047 RID: 71
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr OpenProcess(int dwDesiredAccess, int blnheritHandle, int dwAppProcessId);

		// Token: 0x06000048 RID: 72
		[DllImport("advapi32.dll", SetLastError = true)]
		internal static extern int OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, ref IntPtr TokenHandle);

		// Token: 0x06000049 RID: 73
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetCurrentProcessId();

		// Token: 0x0600004A RID: 74
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int CloseHandle(IntPtr hObject);

		// Token: 0x0600004B RID: 75
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "LookupPrivilegeValueW", SetLastError = true)]
		internal static extern int LookupPrivilegeValue(int lpSystemName, [MarshalAs(UnmanagedType.LPWStr)] string lpName, ref LUID lpLuid);

		// Token: 0x0600004C RID: 76
		[DllImport("advapi32.dll", SetLastError = true)]
		internal static extern int AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPriv, ref TOKEN_PRIVILEGES NewState, int BufferLength, int PreviousState, int ReturnLength);

		// Token: 0x0600004D RID: 77
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "InitiateSystemShutdownW")]
		internal static extern int InitiateSystemShutdown(IntPtr lpMachineName, IntPtr lpMessage, int dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown);

		// Token: 0x0600004E RID: 78
		[DllImport("uxtheme.dll", EntryPoint = "#94")]
		internal static extern int GetImmersiveColorSetCount();
	}
}
