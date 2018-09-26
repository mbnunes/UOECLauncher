// Decompiled with JetBrains decompiler
// Type: UOSALoader.ProcessStream
// Assembly: UOECLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F9631216-9071-42FD-B1D9-2D3BF57CAD0B
// Assembly location: C:\Program Files (x86)\UOECLauncher\UOECLauncher.exe

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UOSALoader
{
  public class ProcessStream : Stream
  {
    private bool m_Open;
    private int m_Position;
    private IntPtr m_Process;
    private IntPtr m_ProcessId;
    private const int ProcessAllAccess = 2035711;

    public ProcessStream()
    {
    }

    public ProcessStream(IntPtr processId)
    {
      this.m_ProcessId = processId;
      this.BeginAccess();
    }

    public bool BeginAccess()
    {
      if (this.m_Open)
        return false;
      this.m_Process = ProcessStream.OpenProcess(2035711, 0, this.m_ProcessId);
      this.m_Open = true;
      return true;
    }

    public override void Close()
    {
      this.EndAccess();
    }

    [DllImport("Kernel32")]
    private static extern int CloseHandle(IntPtr handle);

    public void EndAccess()
    {
      if (!this.m_Open)
        return;
      ProcessStream.CloseHandle(this.m_Process);
      this.m_Open = false;
    }

    public override void Flush()
    {
    }

    public static ProcessStream GetProcessFromHandle(IntPtr handle)
    {
      return new ProcessStream()
      {
        ProcessHandle = handle,
        IsOpened = true
      };
    }

    [DllImport("Kernel32")]
    private static extern IntPtr OpenProcess(int desiredAccess, int inheritHandle, IntPtr processID);

    public override unsafe int Read(byte[] buffer, int offset, int count)
    {
      bool flag = !this.BeginAccess();
      int bytesRead = 0;
      fixed (byte* numPtr = buffer)
        ProcessStream.ReadProcessMemory(this.m_Process, this.m_Position, (void*) (numPtr + offset), count, ref bytesRead);
      this.m_Position += count;
      if (flag)
        this.EndAccess();
      return bytesRead;
    }

    [DllImport("Kernel32")]
    private static extern unsafe int ReadProcessMemory(IntPtr process, int baseAddress, void* buffer, int size, ref int bytesRead);

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:
          this.m_Position = (int) offset;
          break;
        case SeekOrigin.Current:
          this.m_Position += (int) offset;
          break;
        case SeekOrigin.End:
          throw new NotSupportedException();
      }
      return (long) this.m_Position;
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override unsafe void Write(byte[] buffer, int offset, int count)
    {
      bool flag = !this.BeginAccess();
      fixed (byte* numPtr = buffer)
        ProcessStream.WriteProcessMemory(this.m_Process, this.m_Position, (void*) (numPtr + offset), count, 0);
      this.m_Position += count;
      if (!flag)
        return;
      this.EndAccess();
    }

    [DllImport("Kernel32")]
    private static extern unsafe int WriteProcessMemory(IntPtr process, int baseAddress, void* buffer, int size, int bytesWritten);

    public override bool CanRead
    {
      get
      {
        return this.m_Open;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return this.m_Open;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return this.m_Open;
      }
    }

    public bool IsOpened
    {
      get
      {
        return this.m_Open;
      }
      set
      {
        this.m_Open = value;
      }
    }

    public override long Length
    {
      get
      {
        throw new NotSupportedException();
      }
    }

    public override long Position
    {
      get
      {
        return (long) this.m_Position;
      }
      set
      {
        this.Seek(value, SeekOrigin.Begin);
      }
    }

    public IntPtr ProcessHandle
    {
      get
      {
        return this.m_Process;
      }
      set
      {
        this.m_Process = value;
      }
    }
  }
}
