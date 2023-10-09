using System;
using System.Collections.Generic;

namespace redlock
{
	// Token: 0x02000003 RID: 3
	public class ProductPolicy
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000024C4 File Offset: 0x000006C4
		// (set) Token: 0x06000004 RID: 4 RVA: 0x000024CC File Offset: 0x000006CC
		public int Unknown { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000024D5 File Offset: 0x000006D5
		// (set) Token: 0x06000006 RID: 6 RVA: 0x000024DD File Offset: 0x000006DD
		public int Version { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000024E6 File Offset: 0x000006E6
		// (set) Token: 0x06000008 RID: 8 RVA: 0x000024EE File Offset: 0x000006EE
		public Dictionary<string, PolicyItem> Policies { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000009 RID: 9 RVA: 0x000024F7 File Offset: 0x000006F7
		// (set) Token: 0x0600000A RID: 10 RVA: 0x000024FF File Offset: 0x000006FF
		public byte[] EndMarker { get; set; }
	}
}
