// Decompiled with JetBrains decompiler
// Type: UOSALoader.Utility
// Assembly: UOECLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F9631216-9071-42FD-B1D9-2D3BF57CAD0B
// Assembly location: C:\Program Files (x86)\UOECLauncher\UOECLauncher.exe

using Microsoft.Win32;
using System.IO;
using System.Text;

namespace UOSALoader
{
  public class Utility
  {
    public static void FormatBuffer(TextWriter output, Stream input, int length)
    {
      output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
      output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");
      int num1 = 0;
      int num2 = length >> 4;
      int capacity = length & 15;
      int num3 = 0;
      while (num3 < num2)
      {
        StringBuilder stringBuilder1 = new StringBuilder(49);
        StringBuilder stringBuilder2 = new StringBuilder(16);
        for (int index = 0; index < 16; ++index)
        {
          int num4 = input.ReadByte();
          stringBuilder1.Append(num4.ToString("X2"));
          if (index != 7)
            stringBuilder1.Append(' ');
          else
            stringBuilder1.Append("  ");
          if (num4 >= 32 && num4 < 128)
            stringBuilder2.Append((char) num4);
          else
            stringBuilder2.Append('.');
        }
        output.Write(num1.ToString("X4"));
        output.Write("   ");
        output.Write(stringBuilder1.ToString());
        output.Write("  ");
        output.WriteLine(stringBuilder2.ToString());
        ++num3;
        num1 += 16;
      }
      if ((uint) capacity <= 0U)
        return;
      StringBuilder stringBuilder3 = new StringBuilder(49);
      StringBuilder stringBuilder4 = new StringBuilder(capacity);
      for (int index = 0; index < 16; ++index)
      {
        if (index < capacity)
        {
          int num4 = input.ReadByte();
          stringBuilder3.Append(num4.ToString("X2"));
          if (index != 7)
            stringBuilder3.Append(' ');
          else
            stringBuilder3.Append("  ");
          if (num4 >= 32 && num4 < 128)
            stringBuilder4.Append((char) num4);
          else
            stringBuilder4.Append('.');
        }
        else
          stringBuilder3.Append("   ");
      }
      output.Write(num1.ToString("X4"));
      output.Write("   ");
      output.Write(stringBuilder3.ToString());
      output.Write("  ");
      output.WriteLine(stringBuilder4.ToString());
    }

    public static string GetExePath(string subName)
    {
      try
      {
        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(string.Format("SOFTWARE\\{0}", (object) subName));
        if (registryKey == null)
        {
          registryKey = Registry.CurrentUser.OpenSubKey(string.Format("SOFTWARE\\{0}", (object) subName));
          if (registryKey == null)
            return (string) null;
        }
        string path = registryKey.GetValue("ExePath") as string;
        string directoryName;
        if (path == null || path.Length <= 0 || !Directory.Exists(path) && !File.Exists(path))
        {
          directoryName = registryKey.GetValue("InstallDir") as string;
          if (directoryName == null || directoryName.Length <= 0 || !Directory.Exists(directoryName) && !File.Exists(directoryName))
            return (string) null;
        }
        else
          directoryName = Path.GetDirectoryName(path);
        if (directoryName == null || !Directory.Exists(directoryName))
          return (string) null;
        return directoryName;
      }
      catch
      {
        return (string) null;
      }
    }

    public static int Search(Stream pc, byte[] buffer, bool bFile)
    {
      return Utility.Search(pc, buffer, bFile, 0);
    }

    public static int Search(Stream pc, byte[] buffer, bool bFile, int start)
    {
      int num1 = bFile ? 0 : 4194304;
      if (start > 0)
        num1 = start + buffer.Length;
      int count = 4096 + buffer.Length;
      byte[] buffer1 = new byte[count];
      int num2 = 0;
      while (true)
      {
        pc.Seek((long) (num1 + num2 * 4096), SeekOrigin.Begin);
        if (pc.Read(buffer1, 0, count) == count)
        {
          for (int index1 = 0; index1 < 4096; ++index1)
          {
            bool flag = true;
            for (int index2 = 0; flag && index2 < buffer.Length; ++index2)
              flag = (int) buffer[index2] == (int) buffer1[index1 + index2];
            if (flag)
              return num1 + num2 * 4096 + index1;
          }
          ++num2;
        }
        else
          break;
      }
      return 0;
    }

    public static int VersionToInteger(string version)
    {
      string[] strArray = version.Split('.');
      int num = -1;
      try
      {
        num = int.Parse(strArray[3]) + int.Parse(strArray[2]) * 100 + int.Parse(strArray[1]) * 10000 + int.Parse(strArray[0]) * 1000000;
      }
      catch
      {
      }
      return num;
    }
  }
}
