// Decompiled with JetBrains decompiler
// Type: UOECLauncher.Properties.Resources
// Assembly: UOECLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F9631216-9071-42FD-B1D9-2D3BF57CAD0B
// Assembly location: C:\Program Files (x86)\UOECLauncher\UOECLauncher.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace UOECLauncher.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (UOECLauncher.Properties.Resources.resourceMan == null)
          UOECLauncher.Properties.Resources.resourceMan = new ResourceManager("UOECLauncher.Properties.Resources", typeof (UOECLauncher.Properties.Resources).Assembly);
        return UOECLauncher.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return UOECLauncher.Properties.Resources.resourceCulture;
      }
      set
      {
        UOECLauncher.Properties.Resources.resourceCulture = value;
      }
    }

    internal static Bitmap fe77b7b0_b518_473d_82b9_cc4b74141802
    {
      get
      {
        return (Bitmap) UOECLauncher.Properties.Resources.ResourceManager.GetObject("fe77b7b0-b518-473d-82b9-cc4b74141802", UOECLauncher.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap logo
    {
      get
      {
        return (Bitmap) UOECLauncher.Properties.Resources.ResourceManager.GetObject(nameof (logo), UOECLauncher.Properties.Resources.resourceCulture);
      }
    }
  }
}
