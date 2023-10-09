using System;

namespace redlock
{
	// Token: 0x02000004 RID: 4
	public class PolicyItem
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002510 File Offset: 0x00000710
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002518 File Offset: 0x00000718
		public PolicyDataType Type { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002521 File Offset: 0x00000721
		// (set) Token: 0x0600000F RID: 15 RVA: 0x00002529 File Offset: 0x00000729
		public int Flags { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002532 File Offset: 0x00000732
		// (set) Token: 0x06000011 RID: 17 RVA: 0x0000253A File Offset: 0x0000073A
		public int Unknown { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002543 File Offset: 0x00000743
		// (set) Token: 0x06000013 RID: 19 RVA: 0x0000254B File Offset: 0x0000074B
		public object Data { get; set; }
	}
}
