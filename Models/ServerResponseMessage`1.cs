﻿// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Models.ServerResponseMessage`1
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

namespace UHFReaderService.Models
{
  internal class ServerResponseMessage<T>
  {
    public int status { get; set; }

    public string errorCode { get; set; }

    public string message { get; set; }

    public T data { get; set; }
  }
}
