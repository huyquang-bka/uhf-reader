// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Models.LoginModel
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

namespace UHFReaderService.Models
{
  internal class LoginModel
  {
    public string client_id { get; set; }

    public string client_secret { get; set; }

    public string grant_type { get; set; }

    public string username { get; set; }

    public string password { get; set; }

    public LoginModel getDefaultLoginInfo() => new LoginModel()
    {
      client_id = "IAC_Cloud",
      client_secret = "1a82f1d60ba6353bb64a8fb4b05e4bc4",
      grant_type = "password"
    };
  }
}
