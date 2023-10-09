using System;

namespace redlock
{
	// Token: 0x0200000B RID: 11
	internal class PESectionInfo
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600002E RID: 46 RVA: 0x0000551C File Offset: 0x0000371C
		// (set) Token: 0x0600002F RID: 47 RVA: 0x00005524 File Offset: 0x00003724
		public string SectionName { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000030 RID: 48 RVA: 0x0000552D File Offset: 0x0000372D
		// (set) Token: 0x06000031 RID: 49 RVA: 0x00005535 File Offset: 0x00003735
		public int VirtSize { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000032 RID: 50 RVA: 0x0000553E File Offset: 0x0000373E
		// (set) Token: 0x06000033 RID: 51 RVA: 0x00005546 File Offset: 0x00003746
		public int VirtAddr { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000034 RID: 52 RVA: 0x0000554F File Offset: 0x0000374F
		// (set) Token: 0x06000035 RID: 53 RVA: 0x00005557 File Offset: 0x00003757
		public int PhysSize { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00005560 File Offset: 0x00003760
		// (set) Token: 0x06000037 RID: 55 RVA: 0x00005568 File Offset: 0x00003768
		public int PhysAddr { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00005571 File Offset: 0x00003771
		// (set) Token: 0x06000039 RID: 57 RVA: 0x00005579 File Offset: 0x00003779
		public int VirtOffset { get; set; }
	}
}
