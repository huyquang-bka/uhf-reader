// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Properties.Resources
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace UHFReaderService.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
        if (UHFReaderService.Properties.Resources.resourceMan == null)
          UHFReaderService.Properties.Resources.resourceMan = new ResourceManager("UHFReaderService.Properties.Resources", typeof (UHFReaderService.Properties.Resources).Assembly);
        return UHFReaderService.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => UHFReaderService.Properties.Resources.resourceCulture;
      set => UHFReaderService.Properties.Resources.resourceCulture = value;
    }

    internal static Bitmap clear_icon => (Bitmap) UHFReaderService.Properties.Resources.ResourceManager.GetObject(nameof (clear_icon), UHFReaderService.Properties.Resources.resourceCulture);

    internal static Icon radar => (Icon) UHFReaderService.Properties.Resources.ResourceManager.GetObject(nameof (radar), UHFReaderService.Properties.Resources.resourceCulture);

    internal static Bitmap trash_icon => (Bitmap) UHFReaderService.Properties.Resources.ResourceManager.GetObject(nameof (trash_icon), UHFReaderService.Properties.Resources.resourceCulture);
  }
}
