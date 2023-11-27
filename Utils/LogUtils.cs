// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Utils.LogUtils
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using NLog;
using System.Drawing;
using System.Windows.Forms;

namespace UHFReaderService.Utils
{
  public class LogUtils
  {
    private RichTextBox mViewComponent;
    private Logger mLogger;

    public LogUtils(RichTextBox viewComponent, Logger logger)
    {
      this.mViewComponent = viewComponent;
      this.mLogger = logger;
    }

    public LogUtils(Logger logger) => this.mLogger = logger;

    public void logInfo(string message)
    {
      if (this.mLogger != null)
        this.mLogger.Info(message);
      if (this.mViewComponent == null)
        return;
      UHFReaderService.Utils.Utils.showLog(this.mViewComponent, message, Color.Blue);
    }

    public void logDebug(string message)
    {
      if (this.mLogger != null)
        this.mLogger.Debug(message);
      if (this.mViewComponent == null)
        return;
      UHFReaderService.Utils.Utils.showLog(this.mViewComponent, message, Color.Black);
    }

    public void logError(string message)
    {
      if (this.mLogger != null)
        this.mLogger.Error(message);
      if (this.mViewComponent == null)
        return;
      UHFReaderService.Utils.Utils.showLog(this.mViewComponent, message, Color.Red);
    }

    public void showMessageInfo(string message, string title)
    {
      if (this.mLogger != null)
        this.mLogger.Error(message);
      int num = (int) MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
    }

    public void showMessageError(string message, string title)
    {
      if (this.mLogger != null)
        this.mLogger.Error(message);
      int num = (int) MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Hand);
    }
  }
}
