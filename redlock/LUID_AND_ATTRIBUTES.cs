using System;
using System.Runtime.InteropServices;

namespace redlock
{
	// Token: 0x0200000A RID: 10
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct LUID_AND_ATTRIBUTES
	{
		// Token: 0x0400001A RID: 26
		public LUID pLuid;

		// Token: 0x0400001B RID: 27
		public int Attributes;
	}
}
