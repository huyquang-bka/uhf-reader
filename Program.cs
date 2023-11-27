// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Program
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using UHFReaderService.Properties;

namespace UHFReaderService
{
  internal static class Program
  {
    private static Mutex mutex = new Mutex(true, "{E732ACF1-CE4A-43E5-8558-2B72497CFFB1}");

    [STAThread]
    private static void Main()
    {
      Icon radar = Resources.radar;
      if (!Program.mutex.WaitOne(TimeSpan.Zero, true))
        return;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      MainForm mainForm = new MainForm();
      mainForm.Text = "UHF Reader Service";
      mainForm.Icon = radar;
      Application.Run((Form) mainForm);
      Program.mutex.ReleaseMutex();
    }
  }
}
