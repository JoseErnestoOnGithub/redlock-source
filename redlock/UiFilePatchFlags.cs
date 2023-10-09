using System;

namespace redlock
{
	// Token: 0x02000007 RID: 7
	[Flags]
	internal enum UiFilePatchFlags
	{
		// Token: 0x0400000E RID: 14
		None = 0,
		// Token: 0x0400000F RID: 15
		TouchEditInner = 1,
		// Token: 0x04000010 RID: 16
		ItemHeightInPopup = 2,
		// Token: 0x04000011 RID: 17
		TouchSelectPopup = 4,
		// Token: 0x04000012 RID: 18
		WrappingList = 8,
		// Token: 0x04000013 RID: 19
		TouchCarouselScrollBar = 16,
		// Token: 0x04000014 RID: 20
		TouchSwitch = 32,
		// Token: 0x04000015 RID: 21
		TouchEditDeprecated = 64
	}
}
