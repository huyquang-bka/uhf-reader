// Decompiled with JetBrains decompiler
// Type: UHFReaderService.Utils.Utils
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace UHFReaderService.Utils
{
  public static class Utils
  {
    public static T dgvGetSelectedRow<T>(DataGridView gridView)
    {
      int count = gridView.SelectedRows.Count;
      if (count == 0 || count > 1)
        return default (T);
      DataGridViewRow selectedRow = gridView.SelectedRows[0];
      return selectedRow == null ? default (T) : (T) selectedRow.DataBoundItem;
    }

    public static List<T> dgvGetSelectedRows<T>(DataGridView gridView)
    {
      int count = gridView.SelectedRows.Count;
      if (count == 0 || count > 1)
        return (List<T>) null;
      List<T> selectedRows = new List<T>();
      for (int index = 0; index < count; ++index)
      {
        DataGridViewRow selectedRow = gridView.SelectedRows[index];
        selectedRows.Add((T) selectedRow.DataBoundItem);
      }
      return selectedRows;
    }

    public static List<T> dgvGetCheckedRows<T>(DataGridView gridView)
    {
      List<T> checkedRows = new List<T>();
      foreach (DataGridViewRow row in (IEnumerable) gridView.Rows)
      {
        if (Convert.ToBoolean(row.Cells["cbIndex"].EditedFormattedValue))
          checkedRows.Add((T) row.DataBoundItem);
      }
      return checkedRows;
    }

    public static string EncryptConnectionString(string source, string key)
    {
      using (TripleDESCryptoServiceProvider cryptoServiceProvider1 = new TripleDESCryptoServiceProvider())
      {
        using (MD5CryptoServiceProvider cryptoServiceProvider2 = new MD5CryptoServiceProvider())
        {
          byte[] hash = cryptoServiceProvider2.ComputeHash(Encoding.UTF8.GetBytes(key));
          cryptoServiceProvider1.Key = hash;
          cryptoServiceProvider1.Mode = CipherMode.ECB;
          byte[] bytes = Encoding.UTF8.GetBytes(source);
          return Convert.ToBase64String(cryptoServiceProvider1.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }
      }
    }

    public static string DecryptConnectionString(string encrypt, string key)
    {
      using (TripleDESCryptoServiceProvider cryptoServiceProvider1 = new TripleDESCryptoServiceProvider())
      {
        using (MD5CryptoServiceProvider cryptoServiceProvider2 = new MD5CryptoServiceProvider())
        {
          byte[] hash = cryptoServiceProvider2.ComputeHash(Encoding.UTF8.GetBytes(key));
          cryptoServiceProvider1.Key = hash;
          cryptoServiceProvider1.Mode = CipherMode.ECB;
          byte[] inputBuffer = Convert.FromBase64String(encrypt);
          return Encoding.UTF8.GetString(cryptoServiceProvider1.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
        }
      }
    }

    public static void hideGridColumn(DataGridView grid, string[] arrHideColumn)
    {
      foreach (string columnName in arrHideColumn)
      {
        try
        {
          DataGridViewColumn column = grid.Columns[columnName];
          if (column != null)
            column.Visible = false;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
    }

    public static string EnCryptPassword(string strEnCrypt, string key)
    {
      try
      {
        byte[] bytes = Encoding.UTF8.GetBytes(strEnCrypt);
        byte[] hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key));
        TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();
        cryptoServiceProvider.Key = hash;
        cryptoServiceProvider.Mode = CipherMode.ECB;
        cryptoServiceProvider.Padding = PaddingMode.PKCS7;
        byte[] inArray = cryptoServiceProvider.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(inArray, 0, inArray.Length);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return "";
    }

    public static string DeCryptPassword(string strDecypt, string key)
    {
      try
      {
        byte[] inputBuffer = Convert.FromBase64String(strDecypt);
        byte[] hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(key));
        TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();
        cryptoServiceProvider.Key = hash;
        cryptoServiceProvider.Mode = CipherMode.ECB;
        cryptoServiceProvider.Padding = PaddingMode.PKCS7;
        return Encoding.UTF8.GetString(cryptoServiceProvider.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return "";
    }

    public static bool PingHost(string nameOrAddress)
    {
      bool flag = false;
      Ping ping = (Ping) null;
      try
      {
        ping = new Ping();
        flag = ping.Send(nameOrAddress).Status == IPStatus.Success;
      }
      catch (PingException ex)
      {
      }
      finally
      {
        ping?.Dispose();
      }
      return flag;
    }

    public static bool TelnetHost(string nameOrAddress)
    {
      TcpClient tcpClient = (TcpClient) null;
      try
      {
        tcpClient = new TcpClient(nameOrAddress, 18282);
        return true;
      }
      catch (SocketException ex)
      {
        return false;
      }
      finally
      {
        tcpClient?.Close();
      }
    }

    public static DataTable ToDataTable<T>(this IList<T> data)
    {
      PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof (T));
      DataTable dataTable = new DataTable();
      foreach (PropertyDescriptor propertyDescriptor in properties)
      {
        DataColumnCollection columns = dataTable.Columns;
        string name = propertyDescriptor.Name;
        System.Type type = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType);
        if ((object) type == null)
          type = propertyDescriptor.PropertyType;
        columns.Add(name, type);
      }
      foreach (T component in (IEnumerable<T>) data)
      {
        DataRow row = dataTable.NewRow();
        foreach (PropertyDescriptor propertyDescriptor in properties)
          row[propertyDescriptor.Name] = propertyDescriptor.GetValue((object) component) ?? (object) DBNull.Value;
        dataTable.Rows.Add(row);
      }
      return dataTable;
    }

    public static int StringToInt(string value)
    {
      try
      {
        return int.Parse(value);
      }
      catch
      {
        return 0;
      }
    }

    public static bool GetRadidoCheckboxValue(bool radioChecked) => radioChecked;

    public static int GetComboboxValue(object selectedValue) => selectedValue == null ? 0 : int.Parse(selectedValue.ToString());

    public static T GetCCBValue<T>(object selectedValue)
    {
      try
      {
        if (selectedValue == null)
        {
          if (typeof (T) == typeof (int))
            return (T) Convert.ChangeType((object) 0, typeof (T));
          if (typeof (T) == typeof (string))
            return (T) Convert.ChangeType((object) "", typeof (T));
        }
        return (T) Convert.ChangeType(selectedValue, typeof (T));
      }
      catch
      {
        return (T) Activator.CreateInstance(typeof (T), (object[]) null);
      }
    }

    public static T GetSelectedRow<T>(DataGridView dataGridView)
    {
      try
      {
        int count = dataGridView.SelectedRows.Count;
        if (count == 0 || count > 1)
          return (T) Convert.ChangeType((object) null, typeof (T));
        DataGridViewRow selectedRow = dataGridView.SelectedRows[0];
        return selectedRow == null ? (T) Convert.ChangeType((object) null, typeof (T)) : (T) selectedRow.DataBoundItem;
      }
      catch
      {
        return (T) Activator.CreateInstance(typeof (T), (object[]) null);
      }
    }

    public static DateTime GetDatetimePickerValue(DateTimePicker component)
    {
      try
      {
        return component == null || !component.Checked ? DateTime.MinValue : component.Value.Date;
      }
      catch
      {
        return DateTime.MinValue;
      }
    }

    public static string TimeSpanToString(TimeSpan timeSpan)
    {
      try
      {
        return timeSpan.ToString("hh\\:mm\\:ss");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return (string) null;
    }

    public static string TimeSpanToStringModel(TimeSpan timeSpan)
    {
      try
      {
        return "\"" + timeSpan.ToString("hh\\:mm\\:ss") + "\"";
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return (string) null;
    }

    public static byte[] ImageToByte(Image image)
    {
      if (image == null)
        return new byte[0];
      using (MemoryStream memoryStream = new MemoryStream())
      {
        new Bitmap(image).Save((Stream) memoryStream, ImageFormat.Png);
        return memoryStream.ToArray();
      }
    }

    public static byte[] ImageJpegToByte(Image image)
    {
      if (image == null)
        return new byte[0];
      using (MemoryStream memoryStream = new MemoryStream())
      {
        new Bitmap(image).Save((Stream) memoryStream, ImageFormat.Jpeg);
        return memoryStream.ToArray();
      }
    }

    public static Bitmap ByteToImage(byte[] blob)
    {
      try
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          byte[] buffer = blob;
          memoryStream.Write(buffer, 0, Convert.ToInt32(buffer.Length));
          return new Bitmap((Stream) memoryStream, false);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return (Bitmap) null;
    }

    public static string ByteToStringBase64(byte[] blob)
    {
      string stringBase64 = "";
      if (blob != null || blob.Length != 0)
        stringBase64 = Convert.ToBase64String(blob);
      return stringBase64;
    }

    public static Image Base64ToImage(string base64String)
    {
      byte[] buffer = Convert.FromBase64String(base64String);
      using (MemoryStream memoryStream = new MemoryStream(buffer, 0, buffer.Length))
        return Image.FromStream((Stream) memoryStream, true);
    }

    public static T NVL<T>(object input)
    {
      try
      {
        if (input == null)
        {
          if (typeof (T) == typeof (int))
            return (T) Convert.ChangeType((object) 0, typeof (T));
          if (typeof (T) == typeof (string))
            return (T) Convert.ChangeType((object) "", typeof (T));
        }
        return (T) Convert.ChangeType(input, typeof (T));
      }
      catch
      {
        return (T) Activator.CreateInstance(typeof (T), (object[]) null);
      }
    }

    public static T GetListviewValue<T>(ListView listview) => listview.SelectedItems.Count == 0 ? (T) Activator.CreateInstance(typeof (T), (object[]) null) : (T) listview.SelectedItems[0].Tag;

    public static string NvlStringModel(object input)
    {
      try
      {
        return input == null ? "" : "\"" + (string) input + "\"";
      }
      catch
      {
        return "";
      }
    }

    public static bool NotNullValue(object value)
    {
      bool flag = false;
      if (value == null)
        return flag;
      System.Type type = value.GetType();
      if (type.Equals(typeof (string)))
      {
        string str = (string) value;
        if (str != null && str.Length > 0)
          flag = true;
      }
      else if (type.Equals(typeof (int)))
      {
        if ((int) value != 0)
          flag = true;
      }
      else if (type.Equals(typeof (bool)))
      {
        if ((bool) value)
          flag = true;
      }
      else if (type.Equals(typeof (byte[])))
      {
        byte[] numArray = (byte[]) value;
        if (numArray != null && numArray.Length != 0)
          flag = true;
      }
      else if (type.Equals(typeof (DateTime)))
      {
        DateTime dateTime = (DateTime) value;
        if (dateTime != DateTime.MinValue && dateTime != DateTime.MaxValue)
          flag = true;
      }
      return flag;
    }

    public static bool IsNull<T>(object value) => value == null || typeof (T) == typeof (DateTime) && ((DateTime) value == DateTime.MinValue || (DateTime) value == DateTime.MaxValue);

    public static int ConvertBitToInt(bool value) => !value ? 0 : 1;

    public static bool ConvertBitToBool(int value) => value == 1;

    public static BitArray GetFileBits(string filename)
    {
      byte[] numArray;
      using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        numArray = new byte[fileStream.Length];
        fileStream.Read(numArray, 0, (int) fileStream.Length);
      }
      return new BitArray(numArray);
    }

    public static byte[] GetBinaryFile(string filename)
    {
      byte[] buffer = new byte[0];
      if (filename == null || filename == "")
        return buffer;
      using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
      {
        buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, (int) fileStream.Length);
      }
      return buffer;
    }

    public static List<string> GetLocalIPAddress()
    {
      List<string> localIpAddress = new List<string>();
      foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        localIpAddress.Add(address.ToString());
      return localIpAddress;
    }

    public static int GetAvailablePort(int startingPort)
    {
      List<int> intList = new List<int>();
      IPGlobalProperties globalProperties = IPGlobalProperties.GetIPGlobalProperties();
      TcpConnectionInformation[] activeTcpConnections = globalProperties.GetActiveTcpConnections();
      intList.AddRange(((IEnumerable<TcpConnectionInformation>) activeTcpConnections).Where<TcpConnectionInformation>((System.Func<TcpConnectionInformation, bool>) (n => n.LocalEndPoint.Port >= startingPort)).Select<TcpConnectionInformation, int>((System.Func<TcpConnectionInformation, int>) (n => n.LocalEndPoint.Port)));
      IPEndPoint[] activeTcpListeners = globalProperties.GetActiveTcpListeners();
      intList.AddRange(((IEnumerable<IPEndPoint>) activeTcpListeners).Where<IPEndPoint>((System.Func<IPEndPoint, bool>) (n => n.Port >= startingPort)).Select<IPEndPoint, int>((System.Func<IPEndPoint, int>) (n => n.Port)));
      IPEndPoint[] activeUdpListeners = globalProperties.GetActiveUdpListeners();
      intList.AddRange(((IEnumerable<IPEndPoint>) activeUdpListeners).Where<IPEndPoint>((System.Func<IPEndPoint, bool>) (n => n.Port >= startingPort)).Select<IPEndPoint, int>((System.Func<IPEndPoint, int>) (n => n.Port)));
      intList.Sort();
      for (int availablePort = startingPort; availablePort < (int) ushort.MaxValue; ++availablePort)
      {
        if (!intList.Contains(availablePort))
          return availablePort;
      }
      return 0;
    }

    public static void showLogContent(RichTextBox viewComponent, Color color, string message)
    {
      if (viewComponent.InvokeRequired)
      {
        UHFReaderService.Utils.Utils.ChangeMyTextDelegate method = new UHFReaderService.Utils.Utils.ChangeMyTextDelegate(UHFReaderService.Utils.Utils.showLogContent);
        viewComponent.Invoke((Delegate) method, (object) viewComponent, (object) color, (object) message);
      }
      else
      {
        viewComponent.Focus();
        viewComponent.Text = message;
        viewComponent.Refresh();
      }
    }

    public static void showLog(RichTextBox viewComponent, string message, Color color)
    {
      try
      {
        if (viewComponent.InvokeRequired)
        {
          viewComponent.BeginInvoke((Delegate) (() =>
          {
            int textLength = viewComponent.TextLength;
            string text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + message + Environment.NewLine;
            viewComponent.AppendText(text);
            viewComponent.Select(textLength, text.Length);
            viewComponent.SelectionColor = color;
            viewComponent.Select(viewComponent.TextLength, 0);
            viewComponent.Focus();
            viewComponent.Refresh();
          }));
        }
        else
        {
          int textLength = viewComponent.TextLength;
          string text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + message + Environment.NewLine;
          viewComponent.AppendText(text);
          viewComponent.Select(textLength, text.Length);
          viewComponent.SelectionColor = color;
          viewComponent.Select(viewComponent.TextLength, 0);
          viewComponent.Focus();
          viewComponent.Refresh();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    public static void showLog(RichTextBox viewComponent, string message)
    {
      try
      {
        if (viewComponent.InvokeRequired)
        {
          UHFReaderService.Utils.Utils.ChangeMyTextDelegate del = new UHFReaderService.Utils.Utils.ChangeMyTextDelegate(UHFReaderService.Utils.Utils.showLogContent);
          viewComponent.Invoke((Delegate) (() =>
          {
            string text = viewComponent.Text;
            if (text == null)
              return;
            viewComponent.Invoke((Delegate) del, (object) viewComponent, (object) Color.GreenYellow, (object) (text + DateTime.Now.ToString("HH:mm:ss") + ": " + message + "\n"));
            viewComponent.SelectionStart = viewComponent.Text.Length;
            viewComponent.ScrollToCaret();
          }));
        }
        else
        {
          viewComponent.Focus();
          viewComponent.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": " + message + "\n");
          viewComponent.Refresh();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    public static void clearLog(RichTextBox viewComponent)
    {
      if (viewComponent.InvokeRequired)
      {
        UHFReaderService.Utils.Utils.ChangeMyTextDelegate del = new UHFReaderService.Utils.Utils.ChangeMyTextDelegate(UHFReaderService.Utils.Utils.showLogContent);
        viewComponent.Invoke((Delegate) (() =>
        {
          viewComponent.Invoke((Delegate) del, (object) viewComponent, (object) Color.GreenYellow, (object) "");
          viewComponent.SelectionStart = viewComponent.Text.Length;
          viewComponent.ScrollToCaret();
        }));
      }
      else
      {
        viewComponent.Focus();
        viewComponent.Clear();
        viewComponent.Refresh();
      }
    }

    public static void showLogFullTime(RichTextBox viewComponent, string message)
    {
      viewComponent.Focus();
      viewComponent.AppendText("[" + DateTime.Now.ToString("dd/MM/yyyy") + "]\n");
      viewComponent.AppendText(DateTime.Now.ToString("HH:mm:ss") + ": " + message + "\n");
      viewComponent.Refresh();
    }

    private delegate void ChangeMyTextDelegate(RichTextBox ctrl, Color color, string text);
  }
}
