// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Utils.BaseUtils
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace UHFReaderService.Utils
{
  internal class BaseUtils
  {
    public static bool IsIPv4Valid(string ipAddress)
    {
      string pattern = "^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
      return Regex.IsMatch(ipAddress, pattern);
    }

    public static string readConfig(string fieldName)
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "custom.config");
      if (!File.Exists(path))
      {
        using (File.Create(path))
          Console.WriteLine("File created successfully.");
      }
      foreach (string readAllLine in File.ReadAllLines(path))
      {
        if (readAllLine != null && readAllLine != "")
        {
          string[] strArray = readAllLine.Split('=');
          if (strArray != null && strArray.Length > 1 && strArray[0] == fieldName)
            return strArray[1];
        }
      }
      return (string) null;
    }

    public static void writeConfig(string fieldName, string fieldValue)
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "custom.config");
      if (!File.Exists(path))
      {
        using (File.Create(path))
          Console.WriteLine("File created successfully.");
      }
      string[] contents1 = File.ReadAllLines(path);
      for (int index = 0; index < contents1.Length; ++index)
      {
        string str = contents1[index];
        if (str != null && str != "")
        {
          string[] strArray = str.Split('=');
          if (strArray != null && strArray.Length > 1 && strArray[0] == fieldName)
          {
            contents1[index] = fieldName + "=" + fieldValue;
            File.WriteAllLines(path, contents1);
            return;
          }
        }
      }
      string[] contents2 = new string[contents1.Length + 1];
      for (int index = 0; index < contents1.Length; ++index)
        contents2[index] = contents1[index];
      contents2[contents2.Length - 1] = fieldName + "=" + fieldValue;
      File.WriteAllLines(path, contents2);
    }

    public static void writeConfig(string fileName, string fieldName, string fieldValue)
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName + ".config");
      if (!File.Exists(path))
      {
        using (File.Create(path))
          Console.WriteLine("File created successfully.");
      }
      string contents = fieldName + "=" + fieldValue;
      File.WriteAllText(path, contents);
    }
  }
}
