using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace redlock.Properties
{
	// Token: 0x0200000D RID: 13
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x0600004F RID: 79 RVA: 0x0000558A File Offset: 0x0000378A
		internal Resources()
		{
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00005592 File Offset: 0x00003792
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("redlock.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000055BE File Offset: 0x000037BE
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000055C5 File Offset: 0x000037C5
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000053 RID: 83 RVA: 0x000055CD File Offset: 0x000037CD
		internal static byte[] comp1
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("comp1", Resources.resourceCulture);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000054 RID: 84 RVA: 0x000055E8 File Offset: 0x000037E8
		internal static byte[] comp2
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("comp2", Resources.resourceCulture);
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00005603 File Offset: 0x00003803
		internal static byte[] comp3
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("comp3", Resources.resourceCulture);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000056 RID: 86 RVA: 0x0000561E File Offset: 0x0000381E
		internal static byte[] comp4
		{
			get
			{
				return (byte[])Resources.ResourceManager.GetObject("comp4", Resources.resourceCulture);
			}
		}

		// Token: 0x04000022 RID: 34
		private static ResourceManager resourceMan;

		// Token: 0x04000023 RID: 35
		private static CultureInfo resourceCulture;
	}
}
