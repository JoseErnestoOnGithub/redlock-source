using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using redlock.Properties;

namespace redlock
{
	// Token: 0x02000006 RID: 6
	internal class Program
	{
		// Token: 0x06000015 RID: 21 RVA: 0x0000255C File Offset: 0x0000075C
		private static void Main(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = args[i].ToLower();
			}
			if (args.Contains("audit"))
			{
				Program.UnlockInAudit(args);
				return;
			}
			if (args.Contains("auditu"))
			{
				Program.RelockInAudit();
				return;
			}
			Program.StandardRun();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000025B0 File Offset: 0x000007B0
		private static void StandardRun()
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("//");
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("selection");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("/");
			Console.ForegroundColor = foregroundColor;
			Console.WriteLine("1) Install Redpill");
			Console.WriteLine("2) Install Redpill excluding SHSxS");
			Console.WriteLine("3) Install Redpill excluding policies");
			Console.WriteLine("4) Uninstall Redpill");
			Console.WriteLine("5) Exit\n");
			Console.WriteLine("! Make sure you're running this program from the system drive before proceeding\n");
			int num = 0;
			while (num < 1 || num > 5)
			{
				Console.Write("Select a mode: ");
				int.TryParse(Console.ReadLine(), out num);
			}
			if (num == 5)
			{
				return;
			}
			string text = Assembly.GetEntryAssembly().Location + " audit";
			if (num == 4)
			{
				text += "u";
			}
			if (num == 2)
			{
				text += " noshsxs";
			}
			if (num == 3)
			{
				text += " nopol";
			}
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\Setup", true);
			int? num2 = (int?)registryKey.GetValue("SetupType");
			bool flag = true;
			int? num3 = num2;
			int num4 = 2;
			if (((num3.GetValueOrDefault() == num4) & (num3 != null)) && Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\servicing\\Packages", "Microsoft-Windows-ImmersiveBrowser-Package~*~~*.mum").Length != 0)
			{
				if (MessageBox.Show("Rebooting from OOBE on this install may take longer than expected due to Windows servicing, would you like to proceed?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					flag = false;
				}
				else
				{
					text += " queuemie";
				}
			}
			if (flag)
			{
				string text2 = (string)registryKey.GetValue("CmdLine");
				registryKey.SetValue("SetupTypeBak", num2, RegistryValueKind.DWord);
				registryKey.SetValue("CmdLineBak", text2, RegistryValueKind.String);
				registryKey.SetValue("SetupType", 1, RegistryValueKind.DWord);
				registryKey.SetValue("CmdLine", text, RegistryValueKind.String);
				registryKey.Close();
				Console.WriteLine("[i] Rebooting into Setup Mode");
				Program.AdjustPrivilege("SeShutdownPrivilege", true);
				NativeMethods.InitiateSystemShutdown(IntPtr.Zero, IntPtr.Zero, 0, false, true);
				return;
			}
			registryKey.Close();
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000027B8 File Offset: 0x000009B8
		private static void UnlockInAudit(string[] args)
		{
			bool flag = true;
			if (args.Contains("noshsxs"))
			{
				flag = false;
			}
			bool flag2 = true;
			if (args.Contains("nopol"))
			{
				flag2 = false;
			}
			if (flag2)
			{
				Console.WriteLine("[i] Disabling Software Protection Service");
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\sppsvc", true))
				{
					registryKey.SetValue("Start", 4, RegistryValueKind.DWord);
				}
				Console.WriteLine("[i] Installing product policies");
				using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ProductOptions", true))
				{
					byte[] array = (byte[])registryKey2.GetValue("ProductPolicy");
					registryKey2.SetValue("ProductPolicyBkp", array, RegistryValueKind.Binary);
					ProductPolicy productPolicy = LicensingHelper.DeserializeProductPolicy(array);
					for (int i = 1; i < 10; i++)
					{
						string text = string.Format("SLC-Component-RP-0{0}", i);
						Program.SetDWORDPolicyValue(productPolicy.Policies, text, 1, true);
					}
					Program.SetDWORDPolicyValue(productPolicy.Policies, "WSLicensingService-EnableLOBApps", 0, false);
					Program.SetDWORDPolicyValue(productPolicy.Policies, "WinStoreUI-Enabled", 1, false);
					Program.SetDWORDPolicyValue(productPolicy.Policies, "explorer-CanSuppressStartMenuOnLogin", 0, false);
					Program.SetDWORDPolicyValue(productPolicy.Policies, "explorer-ClientLoginExperienceAllowed", 1, false);
					Program.SetDWORDPolicyValue(productPolicy.Policies, "explorer-DefaultLauncherLayout", 0, false);
					Program.SetDWORDPolicyValue(productPolicy.Policies, "Security-SPP-GenuineLocalStatus", 1, true);
					registryKey2.SetValue("ProductPolicy", LicensingHelper.SerializeProductPolicy(productPolicy).ToArray<byte>(), RegistryValueKind.Binary);
				}
			}
			Console.WriteLine("[i] Setting up Redpill values (HKLM)");
			using (RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true))
			{
				registryKey3.SetValue("RPEnabled", 1, RegistryValueKind.DWord);
				registryKey3.SetValue("RPInstalled", 1, RegistryValueKind.DWord);
				registryKey3.SetValue("RPStore", 1, RegistryValueKind.DWord);
			}
			Program.SetUpSmartTweaks();
			Program.SetUpHKCUValues();
			if (flag)
			{
				bool flag3 = IntPtr.Size == 8;
				string text2 = Environment.SystemDirectory + "\\shsxs.dll";
				string text3 = Environment.SystemDirectory + "\\twinui.dll";
				if (!File.Exists(text2) && File.Exists(text3))
				{
					string text4 = text2;
					bool flag4 = true;
					long[] array2 = Program.FindPatternsInFile(text3, new byte[][]
					{
						Encoding.ASCII.GetBytes("RP_GetLayoutManagerBandDependencies"),
						Encoding.ASCII.GetBytes("RP_InitLauncherDataLayer")
					}, false, 0L, 0L);
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[j] <= 0L)
						{
							flag4 = false;
							break;
						}
					}
					using (MemoryStream memoryStream = new MemoryStream(Resources.comp1))
					{
						using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
						{
							byte[] array3 = new byte[2140160];
							gzipStream.Read(array3, 0, array3.Length);
							if (flag3)
							{
								if (flag4)
								{
									array3[20015] = 114;
									array3[20040] = 48;
								}
								File.WriteAllBytes(text2, array3);
								text4 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\shsxs.dll";
							}
							array3 = new byte[2139648];
							gzipStream.Read(array3, 0, array3.Length);
							if (flag4)
							{
								array3[16095] = 114;
								array3[16146] = 48;
							}
							File.WriteAllBytes(flag3 ? text4 : text2, array3);
						}
					}
					long[] array4 = Program.FindPatternsInFile(Environment.SystemDirectory + "\\oobe\\msoobeplugins.dll", new byte[][]
					{
						Encoding.Unicode.GetBytes("OOBEColorolorSet"),
						Encoding.Unicode.GetBytes("GradientColor")
					}, true, 0L, 0L);
					if (array4[0] != 0L || array4[1] != 0L)
					{
						Program.ConformAccentResources(text2, flag3 ? text4 : null, text3);
					}
					int requiredRPVersion = Program.GetRequiredRPVersion(Environment.GetEnvironmentVariable("WINDIR") + "\\explorer.exe");
					if (requiredRPVersion != 26)
					{
						using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true))
						{
							registryKey4.SetValue("RPVersion", requiredRPVersion, RegistryValueKind.DWord);
						}
					}
					UiFilePatchFlags uiFilePatchFlags = UiFilePatchFlags.None;
					if (requiredRPVersion > 23)
					{
						long[] array5 = Program.FindPatternsInFile(Environment.SystemDirectory + "\\dui70.dll", new byte[][]
						{
							Encoding.Unicode.GetBytes("TouchEditInner"),
							Encoding.ASCII.GetBytes("ItemHeightInPopup"),
							Encoding.Unicode.GetBytes("TouchSelectPopupHost"),
							Encoding.Unicode.GetBytes("WrappingList"),
							Encoding.Unicode.GetBytes("TouchCarouselScrollBar"),
							Encoding.ASCII.GetBytes("TouchSwitch"),
							Encoding.ASCII.GetBytes("TouchEdit@")
						}, true, 0L, 0L);
						if (array5[0] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.TouchEditInner;
						}
						if (array5[1] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.ItemHeightInPopup;
						}
						if (array5[2] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.TouchSelectPopup;
						}
						if (array5[3] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.WrappingList;
						}
						if (array5[4] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.TouchCarouselScrollBar;
						}
						if (array5[5] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.TouchSwitch;
						}
						if (array5[6] < 0L)
						{
							uiFilePatchFlags |= UiFilePatchFlags.TouchEditDeprecated;
						}
					}
					if (uiFilePatchFlags != UiFilePatchFlags.None)
					{
						Console.WriteLine("[i] Patching native SHSxS");
						if (uiFilePatchFlags != UiFilePatchFlags.None)
						{
							Program.DoUiFilePatches(text2, uiFilePatchFlags);
							Program.DoDuiMuiPatches(flag3);
						}
						if (flag3)
						{
							Console.WriteLine("[i] Patching WoW SHSxS");
							if (uiFilePatchFlags != UiFilePatchFlags.None)
							{
								Program.DoUiFilePatches(text4, uiFilePatchFlags);
							}
						}
					}
				}
			}
			Directory.SetCurrentDirectory(Environment.SystemDirectory);
			using (MemoryStream memoryStream2 = new MemoryStream(Resources.comp2))
			{
				using (GZipStream gzipStream2 = new GZipStream(memoryStream2, CompressionMode.Decompress))
				{
					byte[] array6 = new byte[1982];
					gzipStream2.Read(array6, 0, array6.Length);
					if (!File.Exists("SysResetRedPill.xml"))
					{
						Console.WriteLine("[i] Writing System Reset manifest");
						File.WriteAllBytes("SysResetRedPill.xml", array6);
					}
					array6 = new byte[595];
					gzipStream2.Read(array6, 0, array6.Length);
					if (!File.Exists("redpill.log"))
					{
						Console.WriteLine("[i] Writing static Redpill setup log");
						File.WriteAllBytes("redpill.log", array6);
					}
					array6 = new byte[944];
					gzipStream2.Read(array6, 0, array6.Length);
					Console.WriteLine("[i] Writing Redpill certificates");
					Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\SystemCertificates\\ROOT\\Certificates\\7721AC1150970D0B6A4B47AAEA73770712C907C5");
					using (RegistryKey registryKey5 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SystemCertificates\\ROOT\\Certificates\\7721AC1150970D0B6A4B47AAEA73770712C907C5", true))
					{
						registryKey5.SetValue("Blob", array6, RegistryValueKind.Binary);
					}
				}
			}
			Program.AttemptMIEInstall(args.Contains("queuemie"));
			Console.WriteLine("[i] Registering Immersive Browser");
			using (RegistryKey registryKey6 = Registry.LocalMachine.OpenSubKey("Software\\RegisteredApplications", true))
			{
				registryKey6.SetValue("Immersive Browser", "SOFTWARE\\Microsoft\\Immersive Browser\\Capabilities", RegistryValueKind.String);
			}
			Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{8E7E60C6-4CE5-476D-9E31-FD450F3F792F}");
			using (RegistryKey registryKey7 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{8E7E60C6-4CE5-476D-9E31-FD450F3F792F}", true))
			{
				registryKey7.SetValue("IsInstalled", 1, RegistryValueKind.DWord);
			}
			using (RegistryKey registryKey8 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true))
			{
				registryKey8.SetValue("MIEInstallResult", 0, RegistryValueKind.DWord);
			}
			using (RegistryKey registryKey9 = Registry.LocalMachine.OpenSubKey("SYSTEM\\Setup", true))
			{
				if (registryKey9.GetValueNames().Contains("SetupTypeBak"))
				{
					Console.WriteLine("[i] Preparing to reboot");
					int? num = (int?)registryKey9.GetValue("SetupTypeBak");
					string text5 = (string)registryKey9.GetValue("CmdLineBak");
					registryKey9.SetValue("SetupType", num, RegistryValueKind.DWord);
					registryKey9.SetValue("CmdLine", text5, RegistryValueKind.String);
					registryKey9.DeleteValue("SetupTypeBak", false);
					registryKey9.DeleteValue("CmdLineBak", false);
				}
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00003028 File Offset: 0x00001228
		private static void RelockInAudit()
		{
			Console.WriteLine("[i] Disabling Software Protection Service");
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\sppsvc", true))
			{
				registryKey.SetValue("Start", 4, RegistryValueKind.DWord);
			}
			Console.WriteLine("[i] Cleaning up product policies");
			using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ProductOptions", true))
			{
				if (registryKey2.GetValueNames().Contains("ProductPolicyBkp"))
				{
					registryKey2.SetValue("ProductPolicy", (byte[])registryKey2.GetValue("ProductPolicyBkp"), RegistryValueKind.Binary);
					registryKey2.DeleteValue("ProductPolicyBkp");
				}
				else
				{
					byte[] array = (byte[])registryKey2.GetValue("ProductPolicy");
					registryKey2.SetValue("ProductPolicyBkp", array, RegistryValueKind.Binary);
					ProductPolicy productPolicy = LicensingHelper.DeserializeProductPolicy(array);
					for (int i = 1; i < 10; i++)
					{
						string text = string.Format("SLC-Component-RP-0{0}", i);
						if (productPolicy.Policies.ContainsKey(text))
						{
							productPolicy.Policies.Remove(text);
						}
					}
					registryKey2.SetValue("ProductPolicy", LicensingHelper.SerializeProductPolicy(productPolicy).ToArray<byte>(), RegistryValueKind.Binary);
				}
			}
			Console.WriteLine("[i] Removing Redpill values (HKLM)");
			using (RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true))
			{
				registryKey3.DeleteValue("RPEnabled", false);
				registryKey3.DeleteValue("RPInstalled", false);
				registryKey3.DeleteValue("RPStore", false);
				registryKey3.DeleteValue("RPVersion", false);
			}
			using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", true))
			{
				registryKey4.DeleteValue("SHSXSWasEnabled", false);
			}
			try
			{
				using (RegistryKey registryKey5 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\GRE_Initialize", true))
				{
					registryKey5.DeleteValue("RemoteFontBootCacheFlags", false);
				}
			}
			catch
			{
			}
			try
			{
				using (RegistryKey registryKey6 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Applets\\Paint\\Capabilities", true))
				{
					registryKey6.DeleteValue("CLSID", false);
				}
			}
			catch
			{
			}
			try
			{
				using (RegistryKey registryKey7 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\AutoplayHandlers", true))
				{
					registryKey7.DeleteValue("ShowFlyout", false);
				}
			}
			catch
			{
			}
			try
			{
				using (RegistryKey registryKey8 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskUI", true))
				{
					registryKey8.DeleteValue("TaskUIEnabled", false);
					registryKey8.DeleteValue("TaskUIRefreshEnabled", false);
					registryKey8.DeleteValue("TaskUIOnImmersive", false);
				}
			}
			catch
			{
			}
			try
			{
				using (RegistryKey registryKey9 = Registry.ClassesRoot.OpenSubKey("CLSID\\{4F12FF5D-D319-4A79-8380-9CC80384DC08}", true))
				{
					registryKey9.DeleteValue("AppID", false);
				}
			}
			catch
			{
			}
			Program.RemoveHKCUValues();
			Directory.SetCurrentDirectory(Environment.SystemDirectory);
			if (File.Exists("shsxs.dll"))
			{
				Console.WriteLine("[i] Removing SHSxS");
				Program.DeleteWithAttrCheck("shsxs.dll");
				File.Delete("shsxs.dll");
				if (IntPtr.Size == 8)
				{
					Program.DeleteWithAttrCheck(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\shsxs.dll");
				}
			}
			if (File.Exists("SysResetRedPill.xml"))
			{
				Console.WriteLine("[i] Removing System Reset manifest");
				File.Delete("SysResetRedPill.xml");
			}
			if (File.Exists("redpill.log"))
			{
				Console.WriteLine("[i] Removing static Redpill setup log");
				File.Delete("redpill.log");
			}
			Console.WriteLine("[i] Removing Redpill certificates");
			Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\SystemCertificates\\ROOT\\Certificates\\7721AC1150970D0B6A4B47AAEA73770712C907C5", false);
			Program.AttemptMIEUninstall();
			Console.WriteLine("[i] Unregistering Immersive Browser");
			using (RegistryKey registryKey10 = Registry.LocalMachine.OpenSubKey("Software\\RegisteredApplications", true))
			{
				registryKey10.DeleteValue("Immersive Browser", false);
			}
			Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{8E7E60C6-4CE5-476D-9E31-FD450F3F792F}", false);
			using (RegistryKey registryKey11 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true))
			{
				registryKey11.DeleteValue("MIEInstallResult", false);
			}
			Program.RevertDuiMuiPatches();
			using (RegistryKey registryKey12 = Registry.LocalMachine.OpenSubKey("SYSTEM\\Setup", true))
			{
				if (registryKey12.GetValueNames().Contains("SetupTypeBak"))
				{
					Console.WriteLine("[i] Preparing to reboot");
					int? num = (int?)registryKey12.GetValue("SetupTypeBak");
					string text2 = (string)registryKey12.GetValue("CmdLineBak");
					registryKey12.SetValue("SetupType", num, RegistryValueKind.DWord);
					registryKey12.SetValue("CmdLine", text2, RegistryValueKind.String);
					registryKey12.DeleteValue("SetupTypeBak", false);
					registryKey12.DeleteValue("CmdLineBak", false);
				}
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00003594 File Offset: 0x00001794
		private static void DeleteWithAttrCheck(string filePath)
		{
			if (File.Exists(filePath))
			{
				FileInfo fileInfo = new FileInfo(filePath);
				if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					fileInfo.Attributes &= ~FileAttributes.ReadOnly;
				}
				File.Delete(filePath);
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000035D0 File Offset: 0x000017D0
		private static void RemoveHKCUValues()
		{
			Console.WriteLine("[i] Removing Redpill values (HKCU)");
			string[] subKeyNames = Registry.Users.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				Console.WriteLine(" -> SID {0}", text);
				try
				{
					using (RegistryKey registryKey = Registry.Users.OpenSubKey(Path.Combine(text, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
					{
						registryKey.DeleteValue("RPEnabled", false);
						registryKey.DeleteValue("RPInstalled", false);
					}
					using (RegistryKey registryKey2 = Registry.Users.OpenSubKey(Path.Combine(text, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"), true))
					{
						registryKey2.DeleteValue("SHSXSWasEnabled", false);
					}
					using (RegistryKey registryKey3 = Registry.Users.OpenSubKey(Path.Combine(text, "Control Panel\\Desktop"), true))
					{
						registryKey3.DeleteValue("FastWallpaperRendering", false);
					}
				}
				catch
				{
				}
			}
			string[] subKeyNames2;
			using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList"))
			{
				subKeyNames2 = registryKey4.GetSubKeyNames();
			}
			Program.AdjustPrivilege("SeBackupPrivilege", true);
			Program.AdjustPrivilege("SeRestorePrivilege", true);
			foreach (string text2 in subKeyNames2)
			{
				if (!subKeyNames.Contains(text2))
				{
					using (RegistryKey registryKey5 = Registry.LocalMachine.OpenSubKey(Path.Combine("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList", text2)))
					{
						if (registryKey5.GetValueNames().Contains("ProfileImagePath"))
						{
							Console.WriteLine(" -> SID {0}", text2);
							string text3 = (string)registryKey5.GetValue("ProfileImagePath");
							if (NativeMethods.RegLoadKey(2147483651U, text2, Path.Combine(text3, "NTuser.dat")) == 0)
							{
								try
								{
									using (RegistryKey registryKey6 = Registry.Users.OpenSubKey(Path.Combine(text2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
									{
										registryKey6.DeleteValue("RPEnabled", false);
										registryKey6.DeleteValue("RPInstalled", false);
									}
									using (RegistryKey registryKey7 = Registry.Users.OpenSubKey(Path.Combine(text2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"), true))
									{
										registryKey7.DeleteValue("SHSXSWasEnabled", false);
									}
									using (RegistryKey registryKey8 = Registry.Users.OpenSubKey(Path.Combine(text2, "Control Panel\\Desktop"), true))
									{
										registryKey8.DeleteValue("FastWallpaperRendering", false);
									}
								}
								finally
								{
									NativeMethods.RegUnLoadKey(2147483651U, text2);
								}
							}
						}
					}
				}
			}
			Console.WriteLine(" -> Default user");
			string text4 = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)).Parent.Parent.FullName + "\\Default\\NTuser.dat";
			if (NativeMethods.RegLoadKey(2147483651U, "Default", text4) == 0)
			{
				try
				{
					using (RegistryKey registryKey9 = Registry.Users.OpenSubKey(Path.Combine("Default", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
					{
						registryKey9.DeleteValue("RPEnabled", false);
						registryKey9.DeleteValue("RPInstalled", false);
					}
					using (RegistryKey registryKey10 = Registry.Users.OpenSubKey(Path.Combine("Default", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"), true))
					{
						registryKey10.DeleteValue("SHSXSWasEnabled", false);
					}
					using (RegistryKey registryKey11 = Registry.Users.OpenSubKey(Path.Combine("Default", "Control Panel\\Desktop"), true))
					{
						registryKey11.DeleteValue("FastWallpaperRendering", false);
					}
				}
				finally
				{
					NativeMethods.RegUnLoadKey(2147483651U, "Default");
				}
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003AC4 File Offset: 0x00001CC4
		private static void AttemptMIEUninstall()
		{
			string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\servicing\\Packages", "Microsoft-Windows-ImmersiveBrowser-Package~*~~*.mum");
			if (files.Length != 0)
			{
				Console.WriteLine("[i] Uninstalling Immersive Browser");
				Process.Start("dism.exe", "/online /NoRestart /Disable-Feature /FeatureName:Immersive-Browser /PackageName:" + files[0].Split(new char[] { '\\' }).Last<string>().Replace(".mum", "")).WaitForExit();
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003B3C File Offset: 0x00001D3C
		private static void RevertDuiMuiPatches()
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			foreach (string text in new string[]
			{
				Environment.SystemDirectory + "\\" + currentUICulture.Name + "\\dui70.dll.mui",
				Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\" + currentUICulture.Name + "\\dui70.dll.mui"
			})
			{
				string text2 = text + ".orig";
				if (File.Exists(text2))
				{
					File.Delete(text);
					File.Move(text2, text);
				}
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003BC8 File Offset: 0x00001DC8
		private static void SetUpSmartTweaks()
		{
			if (Program.FindPatternInFile(Environment.SystemDirectory + "\\WebcamUi.dll", Encoding.Unicode.GetBytes("RemoteFontBootCacheFlags"), true, 0L, 0L) > 0L)
			{
				string text = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\GRE_Initialize";
				Registry.LocalMachine.CreateSubKey(text);
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text, true))
				{
					registryKey.SetValue("RemoteFontBootCacheFlags", 4111, RegistryValueKind.DWord);
				}
			}
			long[] array = Program.FindPatternsInFile(Environment.SystemDirectory + "\\glcnd.exe", new byte[][]
			{
				Encoding.Unicode.GetBytes("{656CF76D-B764-4C23-9CDE-EDEB2514ECA0}"),
				Encoding.Unicode.GetBytes("{D3E34B21-9D75-101A-8C3D-00AA001A1652}")
			}, true, 0L, 0L);
			if (array[0] > 0L)
			{
				string text2 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Applets\\Paint\\Capabilities";
				Registry.LocalMachine.CreateSubKey(text2);
				using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey(text2, true))
				{
					registryKey2.SetValue("CLSID", "{656CF76D-B764-4C23-9CDE-EDEB2514ECA0}", RegistryValueKind.String);
					goto IL_142;
				}
			}
			if (array[1] > 0L)
			{
				string text3 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Applets\\Paint\\Capabilities";
				Registry.LocalMachine.CreateSubKey(text3);
				using (RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey(text3, true))
				{
					registryKey3.SetValue("CLSID", "{D3E34B21-9D75-101A-8C3D-00AA001A1652}", RegistryValueKind.String);
				}
			}
			IL_142:
			if (Program.FindPatternInFile(Environment.SystemDirectory + "\\TaskUI.exe", Encoding.Unicode.GetBytes("TaskUIEnabled"), true, 0L, 0L) > 0L)
			{
				string text4 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskUI";
				Registry.LocalMachine.CreateSubKey(text4);
				using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey(text4, true))
				{
					registryKey4.SetValue("TaskUIEnabled", 1, RegistryValueKind.DWord);
					registryKey4.SetValue("TaskUIRefreshEnabled", 1, RegistryValueKind.DWord);
					registryKey4.SetValue("TaskUIOnImmersive", 1, RegistryValueKind.DWord);
				}
			}
			if (Program.FindPatternInFile(Environment.SystemDirectory + "\\ExplorerFrame.dll", new byte[]
			{
				69, 218, 152, 145, 213, 199, 255, 78, 167, 38,
				120, 252, 84, 125, 255, 83
			}, true, 0L, 0L) > 0L)
			{
				string text5 = "CLSID\\{4F12FF5D-D319-4A79-8380-9CC80384DC08}";
				Registry.ClassesRoot.CreateSubKey(text5);
				using (RegistryKey registryKey5 = Registry.ClassesRoot.OpenSubKey(text5, true))
				{
					registryKey5.SetValue("AppID", "{9198DA45-C7D5-4EFF-A726-78FC547DFF53}", RegistryValueKind.String);
				}
			}
			if (Program.FindPatternInFile(Environment.SystemDirectory + "\\twinui.dll", Encoding.Unicode.GetBytes("ShowFlyout"), true, 0L, 0L) > 0L)
			{
				string text6 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\AutoplayHandlers";
				Registry.LocalMachine.CreateSubKey(text6);
				using (RegistryKey registryKey6 = Registry.LocalMachine.OpenSubKey(text6, true))
				{
					registryKey6.SetValue("ShowFlyout", 1, RegistryValueKind.DWord);
				}
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003EDC File Offset: 0x000020DC
		private static void AttemptMIEInstall(bool queue)
		{
			string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\servicing\\Packages", "Microsoft-Windows-ImmersiveBrowser-Package~*~~*.mum");
			if (files.Length != 0)
			{
				string text = "/online /NoRestart /Enable-Feature /FeatureName:Immersive-Browser /PackageName:" + files[0].Split(new char[] { '\\' }).Last<string>().Replace(".mum", "");
				if (queue)
				{
					Console.WriteLine("[i] Queuing Immersive Browser install");
					string text2 = Environment.GetEnvironmentVariable("WINDIR") + "\\Setup\\Scripts";
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
					}
					File.WriteAllText(text2 + "\\SetupComplete.cmd", "dism.exe " + text);
					return;
				}
				Console.WriteLine("[i] Installing Immersive Browser");
				Process.Start("dism.exe", text).WaitForExit();
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003FA8 File Offset: 0x000021A8
		private static void SetUpHKCUValues()
		{
			Console.WriteLine("[i] Setting up Redpill values (HKCU)");
			bool flag = Program.FindPatternInFile(Environment.SystemDirectory + "\\themecpl.dll", Encoding.Unicode.GetBytes("FastWallpaperRendering"), true, 0L, 0L) > 0L;
			string[] subKeyNames = Registry.Users.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				Console.WriteLine(" -> SID {0}", text);
				try
				{
					using (RegistryKey registryKey = Registry.Users.OpenSubKey(Path.Combine(text, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
					{
						registryKey.SetValue("RPEnabled", 1, RegistryValueKind.DWord);
						registryKey.SetValue("RPInstalled", 1, RegistryValueKind.DWord);
					}
					if (flag)
					{
						using (RegistryKey registryKey2 = Registry.Users.OpenSubKey(Path.Combine(text, "Control Panel\\Desktop"), true))
						{
							registryKey2.SetValue("FastWallpaperRendering", 1, RegistryValueKind.DWord);
						}
					}
				}
				catch
				{
				}
			}
			string[] subKeyNames2;
			using (RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList"))
			{
				subKeyNames2 = registryKey3.GetSubKeyNames();
			}
			Program.AdjustPrivilege("SeBackupPrivilege", true);
			Program.AdjustPrivilege("SeRestorePrivilege", true);
			foreach (string text2 in subKeyNames2)
			{
				if (!subKeyNames.Contains(text2))
				{
					using (RegistryKey registryKey4 = Registry.LocalMachine.OpenSubKey(Path.Combine("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList", text2)))
					{
						if (registryKey4.GetValueNames().Contains("ProfileImagePath"))
						{
							Console.WriteLine(" -> SID {0}", text2);
							string text3 = (string)registryKey4.GetValue("ProfileImagePath");
							if (NativeMethods.RegLoadKey(2147483651U, text2, Path.Combine(text3, "NTuser.dat")) == 0)
							{
								try
								{
									using (RegistryKey registryKey5 = Registry.Users.OpenSubKey(Path.Combine(text2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
									{
										registryKey5.SetValue("RPEnabled", 1, RegistryValueKind.DWord);
										registryKey5.SetValue("RPInstalled", 1, RegistryValueKind.DWord);
									}
									if (flag)
									{
										using (RegistryKey registryKey6 = Registry.Users.OpenSubKey(Path.Combine(text2, "Control Panel\\Desktop"), true))
										{
											registryKey6.SetValue("FastWallpaperRendering", 1, RegistryValueKind.DWord);
										}
									}
								}
								finally
								{
									NativeMethods.RegUnLoadKey(2147483651U, text2);
								}
							}
						}
					}
				}
			}
			Console.WriteLine(" -> Default user");
			string text4 = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)).Parent.Parent.FullName + "\\Default\\NTuser.dat";
			if (NativeMethods.RegLoadKey(2147483651U, "Default", text4) == 0)
			{
				try
				{
					using (RegistryKey registryKey7 = Registry.Users.OpenSubKey(Path.Combine("Default", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer"), true))
					{
						registryKey7.SetValue("RPEnabled", 1, RegistryValueKind.DWord);
						registryKey7.SetValue("RPInstalled", 1, RegistryValueKind.DWord);
					}
					if (flag)
					{
						using (RegistryKey registryKey8 = Registry.Users.OpenSubKey(Path.Combine("Default", "Control Panel\\Desktop"), true))
						{
							registryKey8.SetValue("FastWallpaperRendering", 1, RegistryValueKind.DWord);
						}
					}
				}
				finally
				{
					NativeMethods.RegUnLoadKey(2147483651U, "Default");
				}
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000043A8 File Offset: 0x000025A8
		private static void SetDWORDPolicyValue(Dictionary<string, PolicyItem> Policies, string Name, int Value, bool Overwrite = false)
		{
			if (Policies.ContainsKey(Name) && Overwrite)
			{
				Policies[Name].Data = Value;
				return;
			}
			Policies[Name] = new PolicyItem
			{
				Type = PolicyDataType.DWord,
				Data = Value
			};
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000043E8 File Offset: 0x000025E8
		private static void ConformAccentResources(string patchingNative, string patchingWoW, string twinGuidance)
		{
			Console.WriteLine("[i] Conforming accent resoruces");
			IntPtr intPtr = NativeMethods.LoadLibraryEx(twinGuidance, IntPtr.Zero, 3U);
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			bool flag = NativeMethods.FindResourceEx(intPtr, new IntPtr(2), new IntPtr(4807), 1033) == IntPtr.Zero;
			NativeMethods.FreeLibrary(intPtr);
			byte[] array;
			byte[] array2;
			if (flag)
			{
				array = new byte[53352];
				array2 = new byte[36398];
				using (MemoryStream memoryStream = new MemoryStream(Resources.comp4))
				{
					using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
					{
						gzipStream.Read(array, 0, array.Length);
						gzipStream.Read(array2, 0, array2.Length);
						goto IL_12A;
					}
				}
			}
			if (Program.GetBuildNumber() >= 8102 || NativeMethods.GetImmersiveColorSetCount() == 1)
			{
				return;
			}
			intPtr = NativeMethods.LoadLibraryEx(patchingNative, IntPtr.Zero, 3U);
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			IntPtr intPtr2 = NativeMethods.FindResourceEx(intPtr, "PNG", new IntPtr(5234), 1033);
			int num = NativeMethods.SizeofResource(intPtr, intPtr2);
			array = new byte[num];
			Marshal.Copy(NativeMethods.LoadResource(intPtr, intPtr2), array, 0, num);
			NativeMethods.FreeLibrary(intPtr);
			array2 = array;
			IL_12A:
			IntPtr intPtr3 = NativeMethods.BeginUpdateResource(patchingNative, false);
			NativeMethods.UpdateResource(intPtr3, "PNG", new IntPtr(5231), 1033, array, (uint)array.Length);
			NativeMethods.UpdateResource(intPtr3, "PNG", new IntPtr(5232), 1033, array2, (uint)array2.Length);
			NativeMethods.EndUpdateResource(intPtr3, false);
			if (patchingWoW != null)
			{
				IntPtr intPtr4 = NativeMethods.BeginUpdateResource(patchingWoW, false);
				NativeMethods.UpdateResource(intPtr4, "PNG", new IntPtr(5231), 1033, array, (uint)array.Length);
				NativeMethods.UpdateResource(intPtr4, "PNG", new IntPtr(5232), 1033, array2, (uint)array2.Length);
				NativeMethods.EndUpdateResource(intPtr4, false);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000045D8 File Offset: 0x000027D8
		private static void DoUiFilePatches(string patchingTarget, UiFilePatchFlags patchFlags)
		{
			Console.WriteLine("[i] Patching with flags 0x{0:x}", (int)patchFlags);
			IntPtr intPtr = NativeMethods.LoadLibraryEx(patchingTarget, IntPtr.Zero, 3U);
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			int[] array = new int[]
			{
				3520, 3521, 3522, 3523, 17502, 17542, 17549, 17563, 17576, 17578,
				17582
			};
			string[] array2 = new string[11];
			for (int i = 0; i < 11; i++)
			{
				IntPtr intPtr2 = NativeMethods.FindResourceEx(intPtr, "UIFILE", new IntPtr(array[i]), 1033);
				int num = NativeMethods.SizeofResource(intPtr, intPtr2);
				byte[] array3 = new byte[num];
				Marshal.Copy(NativeMethods.LoadResource(intPtr, intPtr2), array3, 0, num);
				array2[i] = Encoding.ASCII.GetString(array3);
			}
			NativeMethods.FreeLibrary(intPtr);
			for (int j = 0; j < 11; j++)
			{
				if ((patchFlags & UiFilePatchFlags.TouchEditInner) == UiFilePatchFlags.TouchEditInner)
				{
					array2[j] = array2[j].Replace("TouchEditInner", "TouchEdit");
				}
				if ((patchFlags & UiFilePatchFlags.ItemHeightInPopup) == UiFilePatchFlags.ItemHeightInPopup)
				{
					array2[j] = array2[j].Replace(" itemheightinpopup=\"55rp\"", "");
					array2[j] = array2[j].Replace(" itemheightinpopup=\"40rp\"", "");
				}
				if ((patchFlags & UiFilePatchFlags.TouchSelectPopup) == UiFilePatchFlags.TouchSelectPopup)
				{
					array2[j] = array2[j].Replace("TouchSelectPopup visible=\"true\" accessible=\"true\" accrole=\"window\" background=\"ImmersiveControlDarkSelectBackgroundPressed\"/>", "if id=\"atom(TouchSelectPopup)\"><HWNDElement visible=\"true\" accessible=\"true\" accrole=\"list\"/></if> ");
				}
				if ((patchFlags & UiFilePatchFlags.WrappingList) == UiFilePatchFlags.WrappingList)
				{
					array2[j] = array2[j].Replace("WrappingList", "ItemList");
				}
				if ((patchFlags & UiFilePatchFlags.TouchCarouselScrollBar) == UiFilePatchFlags.TouchCarouselScrollBar)
				{
					array2[j] = array2[j].Replace("TouchCarouselScrollBar", "TouchScrollBar");
				}
				if ((patchFlags & UiFilePatchFlags.TouchSwitch) == UiFilePatchFlags.TouchSwitch)
				{
					for (int k = array2[j].IndexOf("<if class=\"DarkToggleClass\">"); k > 0; k = array2[j].IndexOf("<if class=\"DarkToggleClass\">"))
					{
						int num2 = k + 10194;
						array2[j] = array2[j].Substring(0, k) + array2[j].Substring(num2);
					}
				}
				if ((patchFlags & UiFilePatchFlags.TouchEditDeprecated) == UiFilePatchFlags.TouchEditDeprecated)
				{
					array2[j] = array2[j].Replace("<TouchEdit ", "<TouchEdit2 ");
				}
			}
			IntPtr intPtr3 = NativeMethods.BeginUpdateResource(patchingTarget, false);
			for (int l = 0; l < 11; l++)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(array2[l]);
				NativeMethods.UpdateResource(intPtr3, "UIFILE", new IntPtr(array[l]), 1033, bytes, (uint)bytes.Length);
			}
			NativeMethods.EndUpdateResource(intPtr3, false);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00004820 File Offset: 0x00002A20
		private static void DoDuiMuiPatches(bool alsoPatchWow)
		{
			byte[] array = new byte[246];
			byte[] array2 = new byte[550];
			byte[] array3 = new byte[260];
			using (MemoryStream memoryStream = new MemoryStream(Resources.comp3))
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					gzipStream.Read(array, 0, array.Length);
					gzipStream.Read(array2, 0, array2.Length);
					gzipStream.Read(array3, 0, array3.Length);
				}
			}
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			string[] array4;
			if (alsoPatchWow)
			{
				array4 = new string[]
				{
					Environment.SystemDirectory + "\\" + currentUICulture.Name + "\\dui70.dll.mui",
					Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\" + currentUICulture.Name + "\\dui70.dll.mui"
				};
			}
			else
			{
				array4 = new string[] { Environment.SystemDirectory + "\\" + currentUICulture.Name + "\\dui70.dll.mui" };
			}
			foreach (string text in array4)
			{
				IntPtr intPtr = NativeMethods.LoadLibraryEx(text, IntPtr.Zero, 3U);
				IntPtr intPtr2 = NativeMethods.FindResourceEx(intPtr, new IntPtr(6), new IntPtr(9), (ushort)currentUICulture.LCID);
				bool flag = intPtr2 == IntPtr.Zero;
				intPtr2 = NativeMethods.FindResourceEx(intPtr, new IntPtr(6), new IntPtr(8), (ushort)currentUICulture.LCID);
				bool flag2 = intPtr2 == IntPtr.Zero;
				intPtr2 = NativeMethods.FindResourceEx(intPtr, new IntPtr(6), new IntPtr(7), (ushort)currentUICulture.LCID);
				if (intPtr2 != IntPtr.Zero)
				{
					int num = NativeMethods.SizeofResource(intPtr, intPtr2);
					byte[] array6 = new byte[num];
					Marshal.Copy(NativeMethods.LoadResource(intPtr, intPtr2), array6, 0, num);
					NativeMethods.FreeLibrary(intPtr);
					bool flag3 = true;
					for (int j = num - 16; j < num; j++)
					{
						if (array6[j] > 0)
						{
							flag3 = false;
							break;
						}
					}
					if (array6[flag3 ? (num - 18) : (num - 2)] == 37)
					{
						Array.Resize<byte>(ref array6, num + array.Length - (flag3 ? 16 : 0));
						Array.Copy(array, 0, array6, num - (flag3 ? 16 : 0), array.Length);
					}
					IntPtr intPtr3 = IntPtr.Zero;
					if (num != array6.Length)
					{
						if (intPtr3 == IntPtr.Zero)
						{
							intPtr3 = Program.GetResourceUpdaterForMUI(text);
						}
						NativeMethods.UpdateResource(intPtr3, new IntPtr(6), new IntPtr(7), (ushort)currentUICulture.LCID, array6, (uint)array6.Length);
					}
					if (flag2)
					{
						if (intPtr3 == IntPtr.Zero)
						{
							intPtr3 = Program.GetResourceUpdaterForMUI(text);
						}
						NativeMethods.UpdateResource(intPtr3, new IntPtr(6), new IntPtr(8), (ushort)currentUICulture.LCID, array2, (uint)array2.Length);
					}
					if (flag)
					{
						if (intPtr3 == IntPtr.Zero)
						{
							intPtr3 = Program.GetResourceUpdaterForMUI(text);
						}
						NativeMethods.UpdateResource(intPtr3, new IntPtr(6), new IntPtr(9), (ushort)currentUICulture.LCID, array3, (uint)array3.Length);
					}
					if (intPtr3 != IntPtr.Zero)
					{
						NativeMethods.EndUpdateResource(intPtr3, false);
						Program.RevertMuiWorkaround(text);
					}
				}
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00004B60 File Offset: 0x00002D60
		private static bool AdjustPrivilege(string PrivilegeName, bool Enable)
		{
			IntPtr zero = IntPtr.Zero;
			LUID luid = default(LUID);
			TOKEN_PRIVILEGES token_PRIVILEGES = default(TOKEN_PRIVILEGES);
			uint num = 56U;
			IntPtr intPtr = NativeMethods.OpenProcess(2035711, 0, NativeMethods.GetCurrentProcessId());
			if (intPtr == IntPtr.Zero)
			{
				return false;
			}
			if (NativeMethods.OpenProcessToken(intPtr, num, ref zero) == 0)
			{
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(intPtr);
				}
				return false;
			}
			if (NativeMethods.LookupPrivilegeValue(0, PrivilegeName, ref luid) == 0)
			{
				if (zero != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(zero);
				}
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(intPtr);
				}
				return false;
			}
			token_PRIVILEGES.PrivilegeCount = 1;
			token_PRIVILEGES.Privileges.pLuid = luid;
			token_PRIVILEGES.Privileges.Attributes = (Enable ? 2 : 0);
			int num2 = NativeMethods.AdjustTokenPrivileges(zero, 0, ref token_PRIVILEGES, 0, 0, 0);
			if (zero != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(zero);
			}
			if (intPtr != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(intPtr);
			}
			return num2 != 0;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00004C60 File Offset: 0x00002E60
		private static IntPtr GetResourceUpdaterForMUI(string filePath)
		{
			File.Copy(filePath, filePath + ".orig", true);
			long num = Program.FindPatternInFile(filePath, Encoding.Unicode.GetBytes("MUI"), true, 0L, 0L);
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
			{
				fileStream.Seek(num, SeekOrigin.Begin);
				fileStream.WriteByte(65);
			}
			return NativeMethods.BeginUpdateResource(filePath, false);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00004CD8 File Offset: 0x00002ED8
		private static void RevertMuiWorkaround(string filePath)
		{
			long num = Program.FindPatternInFile(filePath, Encoding.Unicode.GetBytes("AUI"), true, 0L, 0L);
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
			{
				fileStream.Seek(num, SeekOrigin.Begin);
				fileStream.WriteByte(77);
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00004D38 File Offset: 0x00002F38
		private static int GetRequiredRPVersion(string filePath)
		{
			int num = int.MaxValue;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					binaryReader.BaseStream.Seek(60L, SeekOrigin.Begin);
					binaryReader.BaseStream.Seek((long)(binaryReader.ReadInt32() + 4), SeekOrigin.Begin);
					ushort num2 = binaryReader.ReadUInt16();
					bool flag = false;
					Console.Write(" -> Architecture: ");
					if (num2 == 34404)
					{
						Console.WriteLine("x64");
						flag = true;
					}
					else
					{
						if (num2 != 332)
						{
							Console.WriteLine("Unknown");
							return num;
						}
						Console.WriteLine("x86");
					}
					ushort num3 = binaryReader.ReadUInt16();
					binaryReader.BaseStream.Seek(flag ? 40L : 44L, SeekOrigin.Current);
					long num4 = (flag ? binaryReader.ReadInt64() : ((long)binaryReader.ReadInt32()));
					binaryReader.BaseStream.Seek(flag ? 208L : 192L, SeekOrigin.Current);
					List<PESectionInfo> list = new List<PESectionInfo>();
					int num5 = 0;
					for (int i = 0; i < (int)num3; i++)
					{
						PESectionInfo pesectionInfo = new PESectionInfo
						{
							SectionName = Encoding.ASCII.GetString(binaryReader.ReadBytes(8)).TrimEnd(new char[1]),
							VirtSize = binaryReader.ReadInt32(),
							VirtAddr = binaryReader.ReadInt32(),
							PhysSize = binaryReader.ReadInt32(),
							PhysAddr = binaryReader.ReadInt32()
						};
						if (num5 < 1 && pesectionInfo.SectionName == ".rsrc")
						{
							num5 = pesectionInfo.PhysAddr;
						}
						pesectionInfo.VirtOffset = pesectionInfo.VirtAddr - pesectionInfo.PhysAddr;
						list.Add(pesectionInfo);
						binaryReader.BaseStream.Seek(16L, SeekOrigin.Current);
					}
					long stringPhysAddr = Program.FindPatternInFile(binaryReader, Encoding.ASCII.GetBytes("RP_VersionCheck"), true, (long)list[0].PhysAddr, (long)num5);
					if (stringPhysAddr > -1L)
					{
						PESectionInfo pesectionInfo2 = list.Where((PESectionInfo x) => stringPhysAddr > (long)x.PhysAddr && stringPhysAddr < (long)(x.PhysAddr + x.PhysSize)).First<PESectionInfo>();
						long num6 = stringPhysAddr + (long)pesectionInfo2.VirtOffset;
						Console.WriteLine(" -> Found RP_VersionCheck at 0x{0:x} (virtual address 0x{1:x} in {2})", stringPhysAddr, num6, pesectionInfo2.SectionName);
						binaryReader.BaseStream.Seek((long)list[0].PhysAddr, SeekOrigin.Begin);
						byte[] array = new byte[15];
						if (flag)
						{
							bool flag2 = false;
							for (;;)
							{
								if (array[8] != 72 || array[9] != 141 || array[10] != 21)
								{
									Array.Copy(array, 1, array, 0, 14);
									try
									{
										array[array.Length - 1] = binaryReader.ReadByte();
									}
									catch
									{
										flag2 = true;
										goto IL_2BA;
									}
									continue;
								}
								IL_2BA:
								if (flag2)
								{
									goto IL_398;
								}
								int num7 = (int)((num6 - (binaryReader.BaseStream.Position + (long)list[0].VirtOffset)) & (long)((ulong)(-1)));
								int num8 = BitConverter.ToInt32(array, 11);
								if (num7 == num8)
								{
									break;
								}
								array[8] = 0;
							}
							Console.WriteLine(" -> Found matching lea rdx");
						}
						else
						{
							num6 += num4;
							bool flag3 = false;
							while (array[10] != 104 || (long)BitConverter.ToInt32(array, 11) != num6)
							{
								Array.Copy(array, 1, array, 0, 14);
								try
								{
									array[array.Length - 1] = binaryReader.ReadByte();
								}
								catch
								{
									flag3 = true;
									break;
								}
							}
							if (!flag3)
							{
								Console.WriteLine(" -> Found matching push offset at 0x{0:x}", binaryReader.BaseStream.Position - 5L);
							}
						}
						IL_398:
						while ((array[10] != 131 || array[11] != 248) && (array[10] != 61 || array[13] != 1 || array[14] != 0))
						{
							Array.Copy(array, 1, array, 0, 14);
							try
							{
								array[array.Length - 1] = binaryReader.ReadByte();
							}
							catch
							{
								return num;
							}
						}
						if (array[14] == 0)
						{
							num = BitConverter.ToInt32(array, 11);
						}
						else
						{
							num = (int)array[12];
						}
						Console.WriteLine(" -> Found cmp eax, {0:x} at 0x{1:x}", num, (int)binaryReader.BaseStream.Position - 5);
					}
					else
					{
						Console.WriteLine(" -> TWinUI doesn't contain RP_VersionCheck");
					}
				}
			}
			return num;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000051EC File Offset: 0x000033EC
		private static long FindPatternInFile(string filePath, byte[] bytePattern, bool getOffset = true, long minOffset = 0L, long maxOffset = 0L)
		{
			if (!File.Exists(filePath))
			{
				return -1L;
			}
			long num;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					num = Program.FindPatternsInFile(binaryReader, new byte[][] { bytePattern }, getOffset, minOffset, maxOffset)[0];
				}
			}
			return num;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00005260 File Offset: 0x00003460
		private static long FindPatternInFile(BinaryReader binReader, byte[] bytePattern, bool getOffset = true, long minOffset = 0L, long maxOffset = 0L)
		{
			return Program.FindPatternsInFile(binReader, new byte[][] { bytePattern }, getOffset, minOffset, maxOffset)[0];
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00005278 File Offset: 0x00003478
		private static long[] FindPatternsInFile(string filePath, byte[][] bytePatterns, bool getOffset = true, long minOffset = 0L, long maxOffset = 0L)
		{
			if (!File.Exists(filePath))
			{
				long[] array = new long[bytePatterns.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = -1L;
				}
				return array;
			}
			long[] array2;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					array2 = Program.FindPatternsInFile(binaryReader, bytePatterns, getOffset, minOffset, maxOffset);
				}
			}
			return array2;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00005300 File Offset: 0x00003500
		private static long[] FindPatternsInFile(BinaryReader binReader, byte[][] bytePatterns, bool getOffset = true, long minOffset = 0L, long maxOffset = 0L)
		{
			long position = binReader.BaseStream.Position;
			string[] array = new string[bytePatterns.Length];
			for (int i = 0; i < bytePatterns.Length; i++)
			{
				array[i] = BitConverter.ToString(bytePatterns[i]);
			}
			long[] array2 = new long[bytePatterns.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = -1L;
			}
			byte[] array3 = new byte[4096];
			if (minOffset > 0L)
			{
				binReader.BaseStream.Seek(minOffset, SeekOrigin.Begin);
			}
			if (maxOffset < 1L)
			{
				maxOffset = binReader.BaseStream.Length;
			}
			while (binReader.BaseStream.Position < maxOffset)
			{
				if (binReader.BaseStream.Position > minOffset)
				{
					Array.Copy(array3, 2048, array3, 0, 2048);
				}
				long num = maxOffset - binReader.BaseStream.Position;
				int num2 = 2048;
				if (num < 2048L)
				{
					num2 = (int)num;
					Array.Resize<byte>(ref array3, 2048 + num2);
				}
				Array.Copy(binReader.ReadBytes(num2), 0, array3, 2048, num2);
				if (getOffset)
				{
					bool flag = true;
					for (int k = 0; k < bytePatterns.Length; k++)
					{
						if (array2[k] <= -1L)
						{
							int num3 = BitConverter.ToString(array3).IndexOf(array[k]);
							if (num3 > -1)
							{
								array2[k] = binReader.BaseStream.Position - (long)array3.Length + (long)((num3 + 1) / 3);
							}
							else
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				else
				{
					bool flag2 = true;
					for (int l = 0; l < bytePatterns.Length; l++)
					{
						if (array2[l] <= -1L)
						{
							if (BitConverter.ToString(array3).Contains(array[l]))
							{
								array2[l] = 1L;
							}
							else
							{
								flag2 = false;
							}
						}
					}
					if (flag2)
					{
						break;
					}
				}
			}
			binReader.BaseStream.Seek(position, SeekOrigin.Begin);
			return array2;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000054C4 File Offset: 0x000036C4
		private static int GetBuildNumber()
		{
			int num;
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
			{
				num = int.Parse((string)registryKey.GetValue("CurrentBuild"));
			}
			return num;
		}
	}
}
