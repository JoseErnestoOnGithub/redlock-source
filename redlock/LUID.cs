using System;
using System.Runtime.InteropServices;

namespace redlock
{
	// Token: 0x02000008 RID: 8
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct LUID
	{
		// Token: 0x04000016 RID: 22
		public int LowPart;

		// Token: 0x04000017 RID: 23
		public int HighPart;
	}
}
