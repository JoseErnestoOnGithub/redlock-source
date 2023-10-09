using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace redlock
{
	// Token: 0x02000002 RID: 2
	public static class LicensingHelper
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static byte[] SerializeProductPolicy(ProductPolicy DeserializedPolicy)
		{
			byte[] array2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					memoryStream.Position = 20L;
					foreach (KeyValuePair<string, PolicyItem> keyValuePair in DeserializedPolicy.Policies)
					{
						long position = memoryStream.Position;
						memoryStream.Position += 2L;
						byte[] array = Encoding.Unicode.GetBytes(keyValuePair.Key);
						binaryWriter.Write((ushort)array.Length);
						PolicyItem value = keyValuePair.Value;
						binaryWriter.Write((short)value.Type);
						memoryStream.Position += 2L;
						binaryWriter.Write(value.Flags);
						binaryWriter.Write(value.Unknown);
						memoryStream.Write(array, 0, array.Length);
						PolicyDataType type = value.Type;
						if (type != PolicyDataType.String)
						{
							if (type != PolicyDataType.DWord)
							{
								array = (byte[])value.Data;
							}
							else
							{
								array = BitConverter.GetBytes((int)value.Data);
							}
						}
						else
						{
							array = Encoding.Unicode.GetBytes((string)value.Data);
						}
						memoryStream.Write(array, 0, array.Length);
						if (array.Length == 1)
						{
							MemoryStream memoryStream2 = memoryStream;
							long position2 = memoryStream2.Position;
							memoryStream2.Position = position2 + 1L;
						}
						int num = (int)memoryStream.Position % 4;
						if (num == 0)
						{
							memoryStream.Position += 4L;
						}
						else
						{
							memoryStream.Position += (long)(4 - num);
						}
						ushort num2 = (ushort)(memoryStream.Position - position);
						memoryStream.Position = position;
						binaryWriter.Write(num2);
						memoryStream.Position += 4L;
						binaryWriter.Write((ushort)array.Length);
						memoryStream.Position += (long)(num2 - 8);
					}
					memoryStream.Write(DeserializedPolicy.EndMarker, 0, DeserializedPolicy.EndMarker.Length);
					memoryStream.Position = 0L;
					binaryWriter.Write((int)memoryStream.Length);
					binaryWriter.Write((int)memoryStream.Length - 20 - DeserializedPolicy.EndMarker.Length);
					binaryWriter.Write(DeserializedPolicy.EndMarker.Length);
					binaryWriter.Write(DeserializedPolicy.Unknown);
					binaryWriter.Write(DeserializedPolicy.Version);
					array2 = memoryStream.ToArray();
				}
			}
			return array2;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000022F0 File Offset: 0x000004F0
		public static ProductPolicy DeserializeProductPolicy(byte[] SerializedPolicy)
		{
			ProductPolicy productPolicy;
			using (MemoryStream memoryStream = new MemoryStream(SerializedPolicy, false))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					binaryReader.ReadInt32();
					int num = binaryReader.ReadInt32();
					int num2 = binaryReader.ReadInt32();
					productPolicy = new ProductPolicy
					{
						Unknown = binaryReader.ReadInt32(),
						Version = binaryReader.ReadInt32()
					};
					int num3 = 20 + num;
					productPolicy.Policies = new Dictionary<string, PolicyItem>();
					while (binaryReader.BaseStream.Position < (long)num3)
					{
						binaryReader.ReadInt16();
						short num4 = binaryReader.ReadInt16();
						PolicyItem policyItem = new PolicyItem
						{
							Type = (PolicyDataType)binaryReader.ReadInt16()
						};
						short num5 = binaryReader.ReadInt16();
						policyItem.Flags = binaryReader.ReadInt32();
						policyItem.Unknown = binaryReader.ReadInt32();
						string @string = Encoding.Unicode.GetString(binaryReader.ReadBytes((int)num4));
						PolicyDataType type = policyItem.Type;
						if (type != PolicyDataType.String)
						{
							if (type != PolicyDataType.DWord)
							{
								policyItem.Data = binaryReader.ReadBytes((int)num5);
							}
							else
							{
								policyItem.Data = binaryReader.ReadInt32();
							}
						}
						else
						{
							policyItem.Data = Encoding.Unicode.GetString(binaryReader.ReadBytes((int)num5));
						}
						if (num5 == 1)
						{
							MemoryStream memoryStream2 = memoryStream;
							long position = memoryStream2.Position;
							memoryStream2.Position = position + 1L;
						}
						productPolicy.Policies[@string] = policyItem;
						int num6 = (int)memoryStream.Position % 4;
						if (num6 == 0)
						{
							memoryStream.Position += 4L;
						}
						else
						{
							memoryStream.Position += (long)(4 - num6);
						}
					}
					productPolicy.EndMarker = binaryReader.ReadBytes(num2);
				}
			}
			return productPolicy;
		}
	}
}
