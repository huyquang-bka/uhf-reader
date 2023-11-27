// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Models.WebSocketRequestMessage
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

namespace UHFReaderService.Models
{
  internal class WebSocketRequestMessage
  {
    public int type { get; set; }

    public string data { get; set; }

    public string requestId { get; set; }
  }
}
