// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Utils.WhiteTagList
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

namespace UHFReaderService.Utils
{
  public class WhiteTagList
  {
    private static string lsTAG = "E28011700000020E26B7CD7B";

    public static bool checkWhiteTagID(string tagID) => WhiteTagList.lsTAG.ToUpper().Contains(tagID.ToUpper());
  }
}
