using System;
using System.Runtime.InteropServices;

namespace redlock
{
	// Token: 0x02000009 RID: 9
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct TOKEN_PRIVILEGES
	{
		// Token: 0x04000018 RID: 24
		public int PrivilegeCount;

		// Token: 0x04000019 RID: 25
		public LUID_AND_ATTRIBUTES Privileges;
	}
}
