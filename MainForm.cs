// Decompiled with JetBrains decompiler
// Type: UHFReaderService.MainForm
// Assembly: UHFReaderService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D48C9746-858E-4C2D-8761-5B530A628C16
// Assembly location: C:\Program Files (x86)\ATIINOVATION\UHF Reader Service\UHFReaderService.exe

using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using ReaderB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UHFReaderService.Models;
using UHFReaderService.Properties;
using UHFReaderService.Utils;

namespace UHFReaderService
{
  public class MainForm : Form
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private LogUtils logUtil;
    private Queue<string> UHFIDQueue = new Queue<string>();
    private List<UHFDataModel> lsTagReceive = new List<UHFDataModel>();
    private bool fIsInventoryScan;
    private int fCmdRet = 30;
    private int fOpenComIndex;
    private byte fComAdr = byte.MaxValue;
    private int frmcomportindex;
    private bool ComOpen;
    private bool fAppClosed;
    private const int BETWEEN_SECOND = 10;
    private int totalTag;
    private static string appName = "UHF Reader Service";
    private Thread t1;
    private Thread t2;
    private Thread t3;
    private IContainer components;
    private Panel panel1;
    private Label lbDeviceStatus;
    private Label label2;
    private Button btnStop;
    private Button btnStart;
    private Label label1;
    private Label label3;
    private NotifyIcon notifyIcon1;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem itemOpenApp;
    private ToolStripMenuItem itemExitApp;
    private GroupBox groupBox1;
    private RichTextBox richTextBox1;
    private Panel panel2;
    private Label label4;
    private TextBox textBox_IP4;
    private TextBox textBox_IP3;
    private Label label5;
    private TextBox textBox_IP2;
    private Label label6;
    private TextBox textBox_IP1;
    private System.Windows.Forms.Timer Timer_Test_;
    private Label lbTotalTagID;
    private Panel panel3;
    private Label label7;
    private PictureBox pictureBox1;
    private Panel panel5;
    private TextBox txtDomainUrl;
    private Label label8;
    private Panel panel4;
    private TextBox txtSecondApart;
    private GroupBox groupBox2;
    private Panel panel6;
    private TextBox txtApi;
    private Label label9;
    private Panel panel8;
    private TextBox txtPassword;
    private Label label11;
    private Panel panel7;
    private TextBox txtUsername;
    private Label label10;
    private GroupBox groupBox4;
    private GroupBox groupBox3;

    public MainForm()
    {
      this.InitializeComponent();
      this.logUtil = new LogUtils(this.richTextBox1, MainForm.logger);
      this.t1 = new Thread((ThreadStart) (() => this.autoPostIDAsync()));
      this.t2 = new Thread((ThreadStart) (() => this.autoRemoveTagExpired()));
      this.t3 = new Thread((ThreadStart) (() => this.autoGetTokenAsync()));
      this.t1.Start();
      this.t2.Start();
      this.t3.Start();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      this.notifyIcon1.BalloonTipTitle = MainForm.appName;
      this.notifyIcon1.BalloonTipText = MainForm.appName;
      this.notifyIcon1.Text = MainForm.appName;
      this.btnStop.Enabled = false;
      this.fAppClosed = false;
      this.fIsInventoryScan = false;
      this.initData();
      if (this.IsRegisteredInStartup())
        return;
      this.RegisterInStartup();
    }

    private void initData()
    {
      string str1 = BaseUtils.readConfig("UHFReaderIPAddress");
      if (str1 != null && BaseUtils.IsIPv4Valid(str1))
        this.fillIpAddress(str1);
      string str2 = BaseUtils.readConfig("UHFReaderSecondApart");
      if (str2 != null)
        this.txtSecondApart.Text = str2;
      else
        this.txtSecondApart.Text = 10.ToString() ?? "";
      string str3 = BaseUtils.readConfig("UHFDomainUrl");
      if (str3 != null)
        this.txtDomainUrl.Text = str3;
      string str4 = BaseUtils.readConfig("UHFApiUrl");
      if (str4 != null)
        this.txtApi.Text = str4;
      string str5 = BaseUtils.readConfig("UHFApiUsername");
      if (str5 != null)
        this.txtUsername.Text = str5;
      string str6 = BaseUtils.readConfig("UHFApiPassword");
      if (str6 == null)
        return;
      this.txtPassword.Text = str6;
    }

    private void btnStart_Click(object sender, EventArgs e) => this.onProcess();

    private void onProcess()
    {
      if (!this.ComOpen)
      {
        string fieldValue1 = this.txtDomainUrl.Text;
        if (fieldValue1 == null || !fieldValue1.Contains("http://") && !fieldValue1.Contains("https://"))
        {
          this.logUtil.logError("Địa chỉ domain không hợp lệ");
          return;
        }
        string fieldValue2 = this.txtApi.Text;
        if (fieldValue2 == null || fieldValue2 == "")
        {
          this.logUtil.logError("Đường dẫn api không hợp lệ " + fieldValue2);
          return;
        }
        string text1 = this.txtUsername.Text;
        if (text1 == null || text1 == "")
        {
          this.logUtil.logError("Username đăng nhâp không được trống");
          return;
        }
        string text2 = this.txtPassword.Text;
        if (text2 == null || text2 == "")
        {
          this.logUtil.logError("Password đăng nhâp không được trống");
          return;
        }
        string ipAddressReader = this.getIpAddressReader();
        if (ipAddressReader == null || ipAddressReader == "" || !BaseUtils.IsIPv4Valid(ipAddressReader))
        {
          this.logUtil.logError("Địa chỉ IP của đầu đọc UHF không hợp lệ");
          return;
        }
        string text3 = this.txtSecondApart.Text;
        if (text3 == null)
        {
          this.logUtil.logError("Thời gian xử lý một TAG giữa hai lần không được trống");
          return;
        }
        if (!int.TryParse(text3, out int _))
        {
          this.logUtil.logError("Thời gian xử lý một TAG giữa hai lần không hợp lệ");
          return;
        }
        try
        {
          this.getApiToken();
          this.connectUHFReader();
          if (fieldValue1.EndsWith("/") && fieldValue2.StartsWith("/"))
            fieldValue1 = fieldValue1.Substring(0, fieldValue1.Length - 1);
          if (!fieldValue1.EndsWith("/") && !fieldValue2.StartsWith("/"))
            fieldValue2 = "/" + fieldValue2;
          BaseUtils.writeConfig("UHFDomainUrl", fieldValue1);
          BaseUtils.writeConfig("UHFApiUrl", fieldValue2);
          BaseUtils.writeConfig("UHFApiUsername", text1);
          BaseUtils.writeConfig("UHFApiPassword", text2);
          BaseUtils.writeConfig("UHFReaderIPAddress", ipAddressReader);
          BaseUtils.writeConfig("UHFReaderSecondApart", text3);
        }
        catch
        {
          this.logUtil.logError("Không thể kết nối tới đầu đọc UHF " + ipAddressReader);
        }
      }
      if (this.ComOpen)
      {
        switch (this.btnStart.Text)
        {
          case "Start":
            this.btnStart.Text = "Pause";
            break;
          case "Pause":
            this.btnStart.Text = "Resume";
            this.fIsInventoryScan = true;
            this.logUtil.logInfo("Tạm dừng");
            break;
          case "Resume":
            this.btnStart.Text = "Pause";
            this.fIsInventoryScan = false;
            this.logUtil.logInfo("Hoạt động");
            break;
        }
        this.btnStop.Enabled = true;
      }
      else
        this.btnStop.Enabled = false;
      this.terminalRunning(this.ComOpen);
    }

    private void saveTimeApart(string timeAart) => BaseUtils.writeConfig("UHFReaderSecondApart", timeAart);

    private void btnStop_Click(object sender, EventArgs e)
    {
      this.disconnectUHFReader();
      if (this.btnStop.Enabled)
      {
        this.fIsInventoryScan = false;
        this.btnStop.Enabled = false;
        this.btnStart.Text = "Start";
        this.logUtil.logInfo("Đã ngắt kết nối tới đầu đọc UHF");
      }
      this.terminalRunning(this.ComOpen);
    }

    private void terminalRunning(bool isRuning)
    {
      this.lbDeviceStatus.BeginInvoke((Delegate) (() => this.lbDeviceStatus.Text = isRuning ? "ON" : "OFF"));
      this.lbDeviceStatus.BeginInvoke((Delegate) (() => this.lbDeviceStatus.ForeColor = isRuning ? System.Drawing.Color.Green : System.Drawing.Color.Red));
    }

    private void connectUHFReader()
    {
      this.fComAdr = Convert.ToByte("FF", 16);
      int num = StaticClassReaderB.OpenNetPort(6000, this.getIpAddressReader(), ref this.fComAdr, ref this.frmcomportindex);
      this.fOpenComIndex = this.frmcomportindex;
      if (num == 0)
      {
        this.ComOpen = true;
        this.logUtil.logInfo("Kết nối đầu đọc UHF thành công");
        this.Timer_Test_.Enabled = true;
      }
      if (num == 53 || num == 48)
      {
        this.logUtil.logError("TCPIP error");
        StaticClassReaderB.CloseNetPort(this.frmcomportindex);
        this.ComOpen = false;
      }
      else
      {
        if (this.fOpenComIndex != -1 && num != 53 && num != 48)
          this.ComOpen = true;
        if (this.fOpenComIndex != -1 || num != 48)
          return;
        this.logUtil.logError("TCPIP Communication Error");
      }
    }

    private void disconnectUHFReader()
    {
      this.fCmdRet = StaticClassReaderB.CloseNetPort(this.frmcomportindex);
      if (this.fCmdRet != 0)
        return;
      this.fOpenComIndex = -1;
      this.ComOpen = false;
    }

    private void Timer_Test__Tick(object sender, EventArgs e)
    {
      if (this.fIsInventoryScan)
        return;
      this.Inventory();
    }

    private void Inventory()
    {
      int CardNum = 0;
      int Totallen = 0;
      byte[] numArray1 = new byte[5000];
      this.fIsInventoryScan = true;
      byte AdrTID = 0;
      byte LenTID = 0;
      byte TIDFlag = 0;
      ListViewItem listViewItem = new ListViewItem();
      this.fCmdRet = StaticClassReaderB.Inventory_G2(ref this.fComAdr, Convert.ToByte("4"), Convert.ToByte("0"), AdrTID, LenTID, TIDFlag, numArray1, ref Totallen, ref CardNum, this.frmcomportindex);
      if (this.fCmdRet == 1 | this.fCmdRet == 2 | this.fCmdRet == 3 | this.fCmdRet == 4 | this.fCmdRet == 251)
      {
        byte[] numArray2 = new byte[Totallen];
        Array.Copy((Array) numArray1, (Array) numArray2, Totallen);
        string hexString = this.ByteArrayToHexString(numArray2);
        int index1 = 0;
        if (CardNum == 0)
        {
          this.fIsInventoryScan = false;
          return;
        }
        for (int index2 = 0; index2 < CardNum; ++index2)
        {
          int num = (int) numArray2[index1];
          string sEPC = hexString.Substring(index1 * 2 + 2, num * 2);
          string str = sEPC;
          hexString.Substring(index1 * 2 + 2 + num * 2, 2);
          index1 = index1 + num + 2;
          if (sEPC.Length != num * 2)
          {
            this.fIsInventoryScan = false;
            return;
          }
          if (WhiteTagList.checkWhiteTagID(sEPC))
          {
            this.fIsInventoryScan = false;
            return;
          }
          UHFDataModel uhfDataModel = this.lsTagReceive.Find((Predicate<UHFDataModel>) (x => x.TagID == sEPC));
          if (uhfDataModel == null)
          {
            this.lsTagReceive.Add(new UHFDataModel()
            {
              TagID = sEPC,
              receiveTime = DateTime.Now
            });
            this.UHFIDQueue.Enqueue(sEPC);
            ++this.totalTag;
          }
          else if ((DateTime.Now - uhfDataModel.receiveTime).TotalSeconds >= (double) int.Parse(this.txtSecondApart.Text))
          {
            uhfDataModel.TagID = sEPC;
            uhfDataModel.receiveTime = DateTime.Now;
            this.lsTagReceive.Add(uhfDataModel);
            this.UHFIDQueue.Enqueue(sEPC);
            ++this.totalTag;
          }
          this.lbTotalTagID.Text = this.totalTag.ToString() ?? "";
        }
      }
      this.fIsInventoryScan = false;
      if (!this.fAppClosed)
        return;
      this.Close();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (e.CloseReason != CloseReason.UserClosing)
        return;
      e.Cancel = true;
      this.Hide();
      this.notifyIcon1.ShowBalloonTip(1000);
      this.notifyIcon1.Visible = true;
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
      if (this.WindowState == FormWindowState.Minimized)
      {
        this.Hide();
        this.notifyIcon1.ShowBalloonTip(1000);
        this.notifyIcon1.Visible = true;
      }
      else
      {
        if (this.WindowState != FormWindowState.Normal)
          return;
        this.notifyIcon1.Visible = false;
      }
    }

    private void notifyIcon1_Click(object sender, EventArgs e) => this.contextMenuStrip.Show(Cursor.Position);

    private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      this.Show();
      this.notifyIcon1.Visible = false;
      this.WindowState = FormWindowState.Normal;
    }

    private void itemExitApp_Click(object sender, EventArgs e)
    {
      if (this.t1 != null && this.t1.IsAlive)
        this.t1.Abort();
      if (this.t2 != null && this.t2.IsAlive)
        this.t2.Abort();
      if (this.t3 != null && this.t3.IsAlive)
        this.t3.Abort();
      Application.Exit();
    }

    private void itemOpenApp_Click(object sender, EventArgs e)
    {
      this.Show();
      this.notifyIcon1.Visible = false;
      this.WindowState = FormWindowState.Normal;
    }

    private bool IsRegisteredInStartup()
    {
      try
      {
        using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
        {
          if (registryKey != null)
          {
            foreach (string valueName in registryKey.GetValueNames())
            {
              if (valueName.Equals(MainForm.appName, StringComparison.OrdinalIgnoreCase))
                return true;
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.logUtil.logError("Lỗi check isStartup: " + ex.Message);
      }
      return false;
    }

    private void RegisterInStartup()
    {
      try
      {
        string executablePath = Application.ExecutablePath;
        using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
          registryKey.SetValue(MainForm.appName, (object) executablePath);
      }
      catch (Exception ex)
      {
        this.logUtil.logError("Lỗi đăng ký ứng dụng khởi động cùng windows: " + ex.Message);
      }
    }

    private void MaskIPAddr(TextBox textBox, KeyPressEventArgs e)
    {
      if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
      {
        if (e.KeyChar != '\b' && textBox.Text.Length == 2)
        {
          string s = textBox.Text + e.KeyChar.ToString();
          if (textBox.Name == "textBox_IP1")
          {
            if (int.Parse(s) > 223)
            {
              int num = (int) MessageBox.Show(s + " giá trị không hợp lệ. Vui lòng nhập số trong khoảng 1 tới 223 ");
              textBox.Text = "223";
              textBox.Focus();
            }
            else
            {
              this.textBox_IP2.Focus();
              this.textBox_IP2.SelectAll();
            }
          }
          else if (textBox.Name == "textBox_IP2")
          {
            if (int.Parse(s) > (int) byte.MaxValue)
            {
              int num = (int) MessageBox.Show(s + " giá trị không hợp lệ. Vui lòng nhập số trong khoảng 1 tới 225 ");
              textBox.Text = "255";
              textBox.Focus();
            }
            else
            {
              this.textBox_IP3.Focus();
              this.textBox_IP3.SelectAll();
            }
          }
          else if (textBox.Name == "textBox_IP3")
          {
            if (int.Parse(s) > (int) byte.MaxValue)
            {
              int num = (int) MessageBox.Show(s + " giá trị không hợp lệ. Vui lòng nhập số trong khoảng 1 tới 225 ");
              textBox.Text = "255";
              textBox.Focus();
            }
            else
            {
              this.textBox_IP4.Focus();
              this.textBox_IP4.SelectAll();
            }
          }
          else
          {
            if (!(textBox.Name == "textBox_IP4") || int.Parse(s) <= (int) byte.MaxValue)
              return;
            int num = (int) MessageBox.Show(s + " giá trị không hợp lệ. Vui lòng nhập số trong khoảng 1 tới 225 ");
            textBox.Text = "255";
            textBox.Focus();
          }
        }
        else
        {
          if (e.KeyChar != '\b')
            return;
          if (textBox.Name == "textBox_IP1" && textBox.Text.Length == 0)
            this.textBox_IP2.Focus();
          else if (textBox.Name == "textBox_IP2" && textBox.Text.Length == 0)
          {
            this.textBox_IP3.Focus();
          }
          else
          {
            if (!(textBox.Name == "textBox_IP3") || textBox.Text.Length != 0)
              return;
            this.textBox_IP4.Focus();
          }
        }
      }
      else
        e.Handled = true;
    }

    private void MaskSecondApart(TextBox textBox, KeyPressEventArgs e)
    {
      if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
      {
        if (e.KeyChar == '\b' || textBox.Text.Length != 2)
          return;
        string s = textBox.Text + e.KeyChar.ToString();
        if (!(textBox.Name == "txtSecondApart") || int.Parse(s) >= 0)
          return;
        int num = (int) MessageBox.Show(s + "  giá trị không hợp lệ. Vui lòng nhập số trong khoảng 1 tới 86400 ");
        textBox.Text = "86400";
        textBox.Focus();
      }
      else
        e.Handled = true;
    }

    private void textBox_IP1_KeyPress(object sender, KeyPressEventArgs e) => this.MaskIPAddr(this.textBox_IP1, e);

    private void textBox_IP2_KeyPress(object sender, KeyPressEventArgs e) => this.MaskIPAddr(this.textBox_IP2, e);

    private void textBox_IP3_KeyPress(object sender, KeyPressEventArgs e) => this.MaskIPAddr(this.textBox_IP3, e);

    private void textBox_IP4_KeyPress(object sender, KeyPressEventArgs e) => this.MaskIPAddr(this.textBox_IP4, e);

    private string getIpAddressReader() => this.textBox_IP1.Text + "." + this.textBox_IP2.Text + "." + this.textBox_IP3.Text + "." + this.textBox_IP4.Text;

    private void txtSecondApart_KeyPress(object sender, KeyPressEventArgs e) => this.MaskSecondApart(this.txtSecondApart, e);

    private string GetReturnCodeDesc(int cmdRet)
    {
      switch (cmdRet)
      {
        case 0:
          return "Operation Successed";
        case 1:
          return "Return before Inventory finished";
        case 2:
          return "the Inventory-scan-time overflow";
        case 3:
          return "More Data";
        case 4:
          return "Reader module MCU is Full";
        case 5:
          return "Access Password Error";
        case 9:
          return "Destroy Password Error";
        case 10:
          return "Destroy Password Error Cannot be Zero";
        case 11:
          return "Tag Not Support the command";
        case 12:
          return "Use the commmand,Access Password Cannot be Zero";
        case 13:
          return "Tag is protected,cannot set it again";
        case 14:
          return "Tag is unprotected,no need to reset it";
        case 16:
          return "There is some locked bytes,write fail";
        case 17:
          return "can not lock it";
        case 18:
          return "is locked,cannot lock it again";
        case 19:
          return "Parameter Save Fail,Can Use Before Power";
        case 20:
          return "Cannot adjust";
        case 21:
          return "Return before Inventory finished";
        case 22:
          return "Inventory-Scan-Time overflow";
        case 23:
          return "More Data";
        case 24:
          return "Reader module MCU is full";
        case 25:
          return "Not Support Command Or AccessPassword Cannot be Zero";
        case 48:
          return "Communication error";
        case 49:
          return "CRC checksummat error";
        case 50:
          return "Return data length error";
        case 51:
          return "Communication busy";
        case 52:
          return "Busy,command is being executed";
        case 53:
          return "ComPort Opened";
        case 54:
          return "ComPort Closed";
        case 55:
          return "Invalid Handle";
        case 56:
          return "Invalid Port";
        case 238:
          return "Return command error";
        case 250:
          return "Get Tag,Poor Communication,Inoperable";
        case 251:
          return "No Tag Operable";
        case 252:
          return "Tag Return ErrorCode";
        case 253:
          return "Command length wrong";
        case 254:
          return "Illegal command";
        case (int) byte.MaxValue:
          return "Parameter Error";
        default:
          return "";
      }
    }

    private string GetErrorCodeDesc(int cmdRet)
    {
      switch (cmdRet)
      {
        case 0:
          return "Other error";
        case 3:
          return "Memory out or pc not support";
        case 4:
          return "Memory Locked and unwritable";
        case 11:
          return "No Power,memory write operation cannot be executed";
        case 15:
          return "Not Special Error,tag not support special errorcode";
        default:
          return "";
      }
    }

    private byte[] HexStringToByteArray(string s)
    {
      s = s.Replace(" ", "");
      byte[] byteArray = new byte[s.Length / 2];
      for (int startIndex = 0; startIndex < s.Length; startIndex += 2)
        byteArray[startIndex / 2] = Convert.ToByte(s.Substring(startIndex, 2), 16);
      return byteArray;
    }

    private string ByteArrayToHexString(byte[] data)
    {
      StringBuilder stringBuilder = new StringBuilder(data.Length * 3);
      foreach (byte num in data)
        stringBuilder.Append(Convert.ToString(num, 16).PadLeft(2, '0'));
      return stringBuilder.ToString().ToUpper();
    }

    private void fillIpAddress(string ipReader)
    {
      string[] strArray = ipReader.Split('.');
      this.textBox_IP1.Text = strArray[0];
      this.textBox_IP2.Text = strArray[1];
      this.textBox_IP3.Text = strArray[2];
      this.textBox_IP4.Text = strArray[3];
    }

    private async Task autoPostIDAsync()
    {
      string tagID = "";
      string apiUrl = "";
      while (true)
      {
        try
        {
          if (this.UHFIDQueue.Count > 0)
          {
            tagID = this.UHFIDQueue.Dequeue();
            ApiDataModel dataObject = new ApiDataModel();
            dataObject.tagID = tagID;
            apiUrl = BaseUtils.readConfig("UHFDomainUrl") + BaseUtils.readConfig("UHFApiUrl");
            if (apiUrl == null || apiUrl == "")
            {
              this.logUtil.logError("Đường dẫn api không hợp lệ " + apiUrl);
              continue;
            }
            await this.postRFID(apiUrl, dataObject);
          }
        }
        catch (Exception ex)
        {
          Exception innerException = ex.InnerException;
          if (innerException != null && innerException.ToString().Contains("Unable to connect to the remote server"))
            this.logUtil.logError("Lỗi gửi TAG ID " + tagID + " tới api " + apiUrl);
          else
            this.logUtil.logError("Lỗi gửi TAG ID " + ex.Message);
        }
        Thread.Sleep(100);
      }
    }

    private async Task postRFID(string apiUrl, ApiDataModel dataObject)
    {
      using (HttpClient httpClient = new HttpClient())
      {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BaseUtils.readConfig("UHFApiToken"));
        string content = JsonConvert.SerializeObject((object) dataObject);
        request.Content = (HttpContent) new StringContent(content, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request);
        if (httpResponseMessage.IsSuccessStatusCode)
          this.logUtil.logInfo("Gửi Tag ID thành công: " + dataObject.tagID);
        else
          this.logUtil.logInfo(string.Format("Lỗi gửi Tag ID {0} mã lỗi {1}", (object) dataObject.tagID, (object) httpResponseMessage.StatusCode));
      }
    }

    private void autoRemoveTagExpired()
    {
      while (true)
      {
        lock (this)
        {
          try
          {
            this.lsTagReceive.RemoveAll((Predicate<UHFDataModel>) (tag => (DateTime.Now - tag.receiveTime).TotalSeconds > (double) int.Parse(this.txtSecondApart.Text)));
          }
          catch (Exception ex)
          {
            Console.WriteLine((object) ex);
          }
          finally
          {
            Thread.Sleep(1000);
          }
        }
      }
    }

    private async Task autoGetTokenAsync()
    {
      while (true)
      {
        try
        {
          await this.getApiToken();
        }
        catch (Exception ex)
        {
          this.logUtil.logError("Error getApiToken " + ex.Message);
        }
        finally
        {
          Thread.Sleep(300000);
        }
      }
    }

    private void pictureBox1_Click(object sender, EventArgs e) => this.richTextBox1.Text = "";

    private async Task getApiToken()
    {
      using (HttpClient httpClient = new HttpClient())
      {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUtils.readConfig("UHFDomainUrl") + "/Service/api/token/auth");
        LoginModel defaultLoginInfo = new LoginModel().getDefaultLoginInfo();
        defaultLoginInfo.username = BaseUtils.readConfig("UHFApiUsername");
        defaultLoginInfo.password = BaseUtils.readConfig("UHFApiPassword");
        string content = JsonConvert.SerializeObject((object) defaultLoginInfo);
        request.Content = (HttpContent) new StringContent(content, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
          try
          {
            BaseUtils.writeConfig("UHFApiToken", JsonConvert.DeserializeObject<LoginResultModel>(await httpResponseMessage.Content.ReadAsStringAsync()).access_token);
            this.logUtil.logInfo("Refresh token success");
          }
          catch (Exception ex)
          {
            this.logUtil.logError("Lỗi lấy token " + ex.Message);
          }
        }
        else
          this.logUtil.logError(string.Format("Lỗi đăng nhập {0}", (object) httpResponseMessage.StatusCode));
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new System.ComponentModel.Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (MainForm));
      this.panel1 = new Panel();
      this.groupBox4 = new GroupBox();
      this.label3 = new Label();
      this.label2 = new Label();
      this.lbDeviceStatus = new Label();
      this.lbTotalTagID = new Label();
      this.groupBox3 = new GroupBox();
      this.label1 = new Label();
      this.panel2 = new Panel();
      this.label4 = new Label();
      this.textBox_IP4 = new TextBox();
      this.textBox_IP3 = new TextBox();
      this.label5 = new Label();
      this.textBox_IP2 = new TextBox();
      this.label6 = new Label();
      this.textBox_IP1 = new TextBox();
      this.label7 = new Label();
      this.panel3 = new Panel();
      this.panel4 = new Panel();
      this.txtSecondApart = new TextBox();
      this.groupBox2 = new GroupBox();
      this.panel8 = new Panel();
      this.txtPassword = new TextBox();
      this.label11 = new Label();
      this.panel7 = new Panel();
      this.txtUsername = new TextBox();
      this.label10 = new Label();
      this.panel6 = new Panel();
      this.txtApi = new TextBox();
      this.label9 = new Label();
      this.panel5 = new Panel();
      this.txtDomainUrl = new TextBox();
      this.label8 = new Label();
      this.pictureBox1 = new PictureBox();
      this.groupBox1 = new GroupBox();
      this.richTextBox1 = new RichTextBox();
      this.btnStop = new Button();
      this.btnStart = new Button();
      this.notifyIcon1 = new NotifyIcon(this.components);
      this.contextMenuStrip = new ContextMenuStrip(this.components);
      this.itemOpenApp = new ToolStripMenuItem();
      this.itemExitApp = new ToolStripMenuItem();
      this.Timer_Test_ = new System.Windows.Forms.Timer(this.components);
      this.panel1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel4.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel8.SuspendLayout();
      this.panel7.SuspendLayout();
      this.panel6.SuspendLayout();
      this.panel5.SuspendLayout();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.groupBox1.SuspendLayout();
      this.contextMenuStrip.SuspendLayout();
      this.SuspendLayout();
      this.panel1.Controls.Add((Control) this.groupBox4);
      this.panel1.Controls.Add((Control) this.groupBox3);
      this.panel1.Controls.Add((Control) this.groupBox2);
      this.panel1.Controls.Add((Control) this.pictureBox1);
      this.panel1.Controls.Add((Control) this.groupBox1);
      this.panel1.Controls.Add((Control) this.btnStop);
      this.panel1.Controls.Add((Control) this.btnStart);
      this.panel1.Dock = DockStyle.Fill;
      this.panel1.Location = new Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(484, 511);
      this.panel1.TabIndex = 0;
      this.groupBox4.Controls.Add((Control) this.label3);
      this.groupBox4.Controls.Add((Control) this.label2);
      this.groupBox4.Controls.Add((Control) this.lbDeviceStatus);
      this.groupBox4.Controls.Add((Control) this.lbTotalTagID);
      this.groupBox4.Location = new Point(3, 141);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new Size(472, 44);
      this.groupBox4.TabIndex = 17;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Status";
      this.label3.AutoSize = true;
      this.label3.Location = new Point(5, 21);
      this.label3.Name = "label3";
      this.label3.Size = new Size(90, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "Trạng thái kết nối";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(256, 21);
      this.label2.Name = "label2";
      this.label2.Size = new Size(64, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Tổng số tag";
      this.lbDeviceStatus.AutoSize = true;
      this.lbDeviceStatus.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.lbDeviceStatus.ForeColor = System.Drawing.Color.Red;
      this.lbDeviceStatus.Location = new Point(109, 21);
      this.lbDeviceStatus.Name = "lbDeviceStatus";
      this.lbDeviceStatus.Size = new Size(30, 13);
      this.lbDeviceStatus.TabIndex = 5;
      this.lbDeviceStatus.Text = "OFF";
      this.lbDeviceStatus.TextAlign = ContentAlignment.MiddleRight;
      this.lbTotalTagID.AutoSize = true;
      this.lbTotalTagID.Location = new Point(347, 21);
      this.lbTotalTagID.Name = "lbTotalTagID";
      this.lbTotalTagID.Size = new Size(13, 13);
      this.lbTotalTagID.TabIndex = 10;
      this.lbTotalTagID.Text = "0";
      this.groupBox3.Controls.Add((Control) this.label1);
      this.groupBox3.Controls.Add((Control) this.panel2);
      this.groupBox3.Controls.Add((Control) this.label7);
      this.groupBox3.Controls.Add((Control) this.panel3);
      this.groupBox3.Location = new Point(3, 80);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(478, 55);
      this.groupBox3.TabIndex = 16;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "UHF reader config";
      this.label1.AutoSize = true;
      this.label1.Location = new Point(6, 25);
      this.label1.Name = "label1";
      this.label1.Size = new Size(55, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "IP Reader";
      this.panel2.BackColor = SystemColors.Window;
      this.panel2.BorderStyle = BorderStyle.Fixed3D;
      this.panel2.Controls.Add((Control) this.label4);
      this.panel2.Controls.Add((Control) this.textBox_IP4);
      this.panel2.Controls.Add((Control) this.textBox_IP3);
      this.panel2.Controls.Add((Control) this.label5);
      this.panel2.Controls.Add((Control) this.textBox_IP2);
      this.panel2.Controls.Add((Control) this.label6);
      this.panel2.Controls.Add((Control) this.textBox_IP1);
      this.panel2.Location = new Point(70, 21);
      this.panel2.Name = "panel2";
      this.panel2.Size = new Size(162, 21);
      this.panel2.TabIndex = 9;
      this.label4.AutoSize = true;
      this.label4.BackColor = SystemColors.Window;
      this.label4.Location = new Point(115, 2);
      this.label4.Name = "label4";
      this.label4.Size = new Size(10, 13);
      this.label4.TabIndex = 8;
      this.label4.Text = ".";
      this.textBox_IP4.BorderStyle = BorderStyle.None;
      this.textBox_IP4.Location = new Point(126, 1);
      this.textBox_IP4.MaxLength = 3;
      this.textBox_IP4.Name = "textBox_IP4";
      this.textBox_IP4.Size = new Size(25, 13);
      this.textBox_IP4.TabIndex = 7;
      this.textBox_IP4.TextAlign = HorizontalAlignment.Center;
      this.textBox_IP4.KeyPress += new KeyPressEventHandler(this.textBox_IP4_KeyPress);
      this.textBox_IP3.BorderStyle = BorderStyle.None;
      this.textBox_IP3.Location = new Point(86, 1);
      this.textBox_IP3.MaxLength = 3;
      this.textBox_IP3.Name = "textBox_IP3";
      this.textBox_IP3.Size = new Size(25, 13);
      this.textBox_IP3.TabIndex = 6;
      this.textBox_IP3.TextAlign = HorizontalAlignment.Center;
      this.textBox_IP3.KeyPress += new KeyPressEventHandler(this.textBox_IP3_KeyPress);
      this.label5.AutoSize = true;
      this.label5.BackColor = SystemColors.Window;
      this.label5.Location = new Point(75, 2);
      this.label5.Name = "label5";
      this.label5.Size = new Size(10, 13);
      this.label5.TabIndex = 5;
      this.label5.Text = ".";
      this.textBox_IP2.BorderStyle = BorderStyle.None;
      this.textBox_IP2.Location = new Point(46, 1);
      this.textBox_IP2.MaxLength = 3;
      this.textBox_IP2.Name = "textBox_IP2";
      this.textBox_IP2.Size = new Size(25, 13);
      this.textBox_IP2.TabIndex = 4;
      this.textBox_IP2.TextAlign = HorizontalAlignment.Center;
      this.textBox_IP2.KeyPress += new KeyPressEventHandler(this.textBox_IP2_KeyPress);
      this.label6.AutoSize = true;
      this.label6.BackColor = SystemColors.Window;
      this.label6.Location = new Point(35, 2);
      this.label6.Name = "label6";
      this.label6.Size = new Size(10, 13);
      this.label6.TabIndex = 3;
      this.label6.Text = ".";
      this.textBox_IP1.BorderStyle = BorderStyle.None;
      this.textBox_IP1.Location = new Point(5, 1);
      this.textBox_IP1.MaxLength = 3;
      this.textBox_IP1.Name = "textBox_IP1";
      this.textBox_IP1.Size = new Size(25, 13);
      this.textBox_IP1.TabIndex = 2;
      this.textBox_IP1.TextAlign = HorizontalAlignment.Center;
      this.textBox_IP1.KeyPress += new KeyPressEventHandler(this.textBox_IP1_KeyPress);
      this.label7.AutoSize = true;
      this.label7.Location = new Point(256, 25);
      this.label7.Name = "label7";
      this.label7.Size = new Size(63, 13);
      this.label7.TabIndex = 11;
      this.label7.Text = "Số giây chờ";
      this.panel3.BackColor = SystemColors.Window;
      this.panel3.BorderStyle = BorderStyle.Fixed3D;
      this.panel3.Controls.Add((Control) this.panel4);
      this.panel3.Location = new Point(350, 19);
      this.panel3.Name = "panel3";
      this.panel3.Size = new Size(121, 21);
      this.panel3.TabIndex = 12;
      this.panel4.BackColor = SystemColors.Window;
      this.panel4.BorderStyle = BorderStyle.Fixed3D;
      this.panel4.Controls.Add((Control) this.txtSecondApart);
      this.panel4.Location = new Point(-2, -2);
      this.panel4.Name = "panel4";
      this.panel4.Size = new Size(121, 21);
      this.panel4.TabIndex = 13;
      this.txtSecondApart.BorderStyle = BorderStyle.None;
      this.txtSecondApart.Location = new Point(5, 1);
      this.txtSecondApart.MaxLength = 6;
      this.txtSecondApart.Name = "txtSecondApart";
      this.txtSecondApart.Size = new Size(100, 13);
      this.txtSecondApart.TabIndex = 2;
      this.txtSecondApart.KeyPress += new KeyPressEventHandler(this.txtSecondApart_KeyPress);
      this.groupBox2.Controls.Add((Control) this.panel8);
      this.groupBox2.Controls.Add((Control) this.label11);
      this.groupBox2.Controls.Add((Control) this.panel7);
      this.groupBox2.Controls.Add((Control) this.label10);
      this.groupBox2.Controls.Add((Control) this.panel6);
      this.groupBox2.Controls.Add((Control) this.label9);
      this.groupBox2.Controls.Add((Control) this.panel5);
      this.groupBox2.Controls.Add((Control) this.label8);
      this.groupBox2.Location = new Point(3, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(478, 71);
      this.groupBox2.TabIndex = 15;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Api config";
      this.panel8.BackColor = SystemColors.Window;
      this.panel8.BorderStyle = BorderStyle.Fixed3D;
      this.panel8.Controls.Add((Control) this.txtPassword);
      this.panel8.Location = new Point(311, 42);
      this.panel8.Name = "panel8";
      this.panel8.Size = new Size(161, 21);
      this.panel8.TabIndex = 20;
      this.txtPassword.BorderStyle = BorderStyle.None;
      this.txtPassword.Location = new Point(5, 1);
      this.txtPassword.MaxLength = 20;
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new Size(150, 13);
      this.txtPassword.TabIndex = 2;
      this.label11.AutoSize = true;
      this.label11.Location = new Point(256, 46);
      this.label11.Name = "label11";
      this.label11.Size = new Size(53, 13);
      this.label11.TabIndex = 19;
      this.label11.Text = "Password";
      this.panel7.BackColor = SystemColors.Window;
      this.panel7.BorderStyle = BorderStyle.Fixed3D;
      this.panel7.Controls.Add((Control) this.txtUsername);
      this.panel7.Location = new Point(72, 42);
      this.panel7.Name = "panel7";
      this.panel7.Size = new Size(160, 21);
      this.panel7.TabIndex = 18;
      this.txtUsername.BorderStyle = BorderStyle.None;
      this.txtUsername.Location = new Point(5, 1);
      this.txtUsername.MaxLength = 20;
      this.txtUsername.Name = "txtUsername";
      this.txtUsername.Size = new Size(150, 13);
      this.txtUsername.TabIndex = 2;
      this.label10.AutoSize = true;
      this.label10.Location = new Point(5, 46);
      this.label10.Name = "label10";
      this.label10.Size = new Size(55, 13);
      this.label10.TabIndex = 17;
      this.label10.Text = "Username";
      this.panel6.BackColor = SystemColors.Window;
      this.panel6.BorderStyle = BorderStyle.Fixed3D;
      this.panel6.Controls.Add((Control) this.txtApi);
      this.panel6.Location = new Point(311, 15);
      this.panel6.Name = "panel6";
      this.panel6.Size = new Size(160, 21);
      this.panel6.TabIndex = 16;
      this.txtApi.BorderStyle = BorderStyle.None;
      this.txtApi.Location = new Point(5, 1);
      this.txtApi.MaxLength = 250;
      this.txtApi.Name = "txtApi";
      this.txtApi.Size = new Size(150, 13);
      this.txtApi.TabIndex = 2;
      this.label9.AutoSize = true;
      this.label9.Location = new Point(256, 18);
      this.label9.Name = "label9";
      this.label9.Size = new Size(22, 13);
      this.label9.TabIndex = 15;
      this.label9.Text = "Api";
      this.panel5.BackColor = SystemColors.Window;
      this.panel5.BorderStyle = BorderStyle.Fixed3D;
      this.panel5.Controls.Add((Control) this.txtDomainUrl);
      this.panel5.Location = new Point(72, 15);
      this.panel5.Name = "panel5";
      this.panel5.Size = new Size(160, 21);
      this.panel5.TabIndex = 14;
      this.txtDomainUrl.BorderStyle = BorderStyle.None;
      this.txtDomainUrl.Location = new Point(5, 1);
      this.txtDomainUrl.MaxLength = 250;
      this.txtDomainUrl.Name = "txtDomainUrl";
      this.txtDomainUrl.Size = new Size(150, 13);
      this.txtDomainUrl.TabIndex = 2;
      this.label8.AutoSize = true;
      this.label8.Location = new Point(5, 19);
      this.label8.Name = "label8";
      this.label8.Size = new Size(43, 13);
      this.label8.TabIndex = 13;
      this.label8.Text = "Domain";
      this.pictureBox1.Image = (Image) Resources.trash_icon;
      this.pictureBox1.Location = new Point(461, 488);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(20, 20);
      this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
      this.pictureBox1.TabIndex = 1;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.Click += new EventHandler(this.pictureBox1_Click);
      this.groupBox1.Controls.Add((Control) this.richTextBox1);
      this.groupBox1.Location = new Point(3, 220);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(478, 288);
      this.groupBox1.TabIndex = 8;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Log";
      this.richTextBox1.BackColor = SystemColors.ControlLightLight;
      this.richTextBox1.Dock = DockStyle.Fill;
      this.richTextBox1.Location = new Point(3, 16);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.Size = new Size(472, 269);
      this.richTextBox1.TabIndex = 0;
      this.richTextBox1.Text = "";
      this.btnStop.Location = new Point(251, 194);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new Size(75, 23);
      this.btnStop.TabIndex = 3;
      this.btnStop.Text = "Stop";
      this.btnStop.UseVisualStyleBackColor = true;
      this.btnStop.Click += new EventHandler(this.btnStop_Click);
      this.btnStart.Location = new Point(159, 194);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new Size(75, 23);
      this.btnStart.TabIndex = 2;
      this.btnStart.Text = "Start";
      this.btnStart.UseVisualStyleBackColor = true;
      this.btnStart.Click += new EventHandler(this.btnStart_Click);
      this.notifyIcon1.Icon = (Icon) componentResourceManager.GetObject("notifyIcon1.Icon");
      this.notifyIcon1.Text = "notifyIcon1";
      this.notifyIcon1.Click += new EventHandler(this.notifyIcon1_Click);
      this.notifyIcon1.MouseDoubleClick += new MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
      this.contextMenuStrip.Items.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.itemOpenApp,
        (ToolStripItem) this.itemExitApp
      });
      this.contextMenuStrip.Name = "contextMenuStrip";
      this.contextMenuStrip.Size = new Size(148, 48);
      this.itemOpenApp.Name = "itemOpenApp";
      this.itemOpenApp.Size = new Size(147, 22);
      this.itemOpenApp.Text = "Mở ứng dụng";
      this.itemOpenApp.Click += new EventHandler(this.itemOpenApp_Click);
      this.itemExitApp.Name = "itemExitApp";
      this.itemExitApp.Size = new Size(147, 22);
      this.itemExitApp.Text = "Thoát";
      this.itemExitApp.Click += new EventHandler(this.itemExitApp_Click);
      this.Timer_Test_.Tick += new EventHandler(this.Timer_Test__Tick);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(484, 511);
      this.ContextMenuStrip = this.contextMenuStrip;
      this.Controls.Add((Control) this.panel1);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.Name = nameof (MainForm);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "UHF Reader Service";
      this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new EventHandler(this.MainForm_Load);
      this.Resize += new EventHandler(this.MainForm_Resize);
      this.panel1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.panel4.ResumeLayout(false);
      this.panel4.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.panel8.ResumeLayout(false);
      this.panel8.PerformLayout();
      this.panel7.ResumeLayout(false);
      this.panel7.PerformLayout();
      this.panel6.ResumeLayout(false);
      this.panel6.PerformLayout();
      this.panel5.ResumeLayout(false);
      this.panel5.PerformLayout();
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.contextMenuStrip.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
