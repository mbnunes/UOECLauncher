// Decompiled with JetBrains decompiler
// Type: UOLauncher.Form1
// Assembly: UOECLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F9631216-9071-42FD-B1D9-2D3BF57CAD0B
// Assembly location: C:\Program Files (x86)\UOECLauncher\UOECLauncher.exe

using EasyHook;
using InjectionSeed;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using UOSALoader;

namespace UOLauncher
{
  public class Form1 : Form
  {
    private string config_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UOECLauncher", "launcher.xml");
    private IContainer components = (IContainer) null;
    private const string c_Seed = "connection.dll";
    private static string s_ChannelName;
    private List<Form1.Server> m_Server;
    private Button quit;
    private Button okay;
    private GroupBox groupBox2;
    private TextBox port;
    private Label label3;
    private ComboBox serverList;
    private Label label4;
    private LinkLabel linkLabel1;
    private GroupBox groupBox1;
    private Label lblClient;
    private LinkLabel lnkOpen;
    private TextBox txtUokrPath;
    private OpenFileDialog ofdUOKRClient;

    public Form1()
    {
            InitializeComponent();
    }

    public static int ToInt32(string str, int def)
    {
      if (str == null)
        return def;
      try
      {
        if (str.Length > 2 && str.Substring(0, 2).ToLower() == "0x")
          return Convert.ToInt32(str.Substring(2), 16);
        return Convert.ToInt32(str);
      }
      catch
      {
        return def;
      }
    }

    private void okay_Click(object sender, EventArgs e)
    {
      string serverip = this.serverList.Text.Trim();
      int serverport = Form1.ToInt32(this.port.Text.Trim(), 0);
      if (serverip == "")
      {
        int num1 = (int) MessageBox.Show("Enter the server IP adress!");
      }
      else if (serverport <= 0 || serverport > (int) ushort.MaxValue)
      {
        int num2 = (int) MessageBox.Show("Enter a valid port!");
      }
      else
      {
        IPAddress address = (IPAddress) null;
        try
        {
          IPAddress[] hostAddresses = Dns.GetHostAddresses(serverip);
          if ((uint) hostAddresses.Length > 0U)
          {
            address = hostAddresses[0];
          }
          else
          {
            int num3 = (int) MessageBox.Show("IP address not found!");
            return;
          }
        }
        catch (SocketException ex)
        {
          int num3 = (int) MessageBox.Show(ex.Message);
        }
        catch (Exception ex)
        {
          int num3 = (int) MessageBox.Show(ex.Message);
        }
        if (!File.Exists(Path.Combine(this.txtUokrPath.Text, "UOSA.exe")))
        {
          int num4 = (int) MessageBox.Show("UOSA.exe does not exist.");
        }
        else
        {
          try
          {
            RemoteHooking.IpcCreateServer<SocketCommunication>(ref Form1.s_ChannelName, WellKnownObjectMode.SingleCall);
            string str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "connection.dll");
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
              FileName = Path.Combine(this.txtUokrPath.Text, "UOSA.exe"),
              WorkingDirectory = this.txtUokrPath.Text
            };
            if (!process.Start())
            {
              int num3 = (int) MessageBox.Show("Cannot start the client !", Application.ProductName + " Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
              uint num5 = this.ConvertIpAdress(address);
              ushort num6 = this.ConvertPort(serverport);
              process.WaitForInputIdle();
              RemoteHooking.Inject(process.Id, str, str, (object) Form1.s_ChannelName, (object) num5, (object) num6);
              Form1.PathOrLaunch(process.Id);
              if (this.m_Server.Find((Predicate<Form1.Server>) (s =>
              {
                if (s.IP == serverip)
                  return s.Port == serverport;
                return false;
              })) == null)
                this.m_Server.Add(new Form1.Server()
                {
                  IP = serverip,
                  Port = serverport
                });
              XElement xelement = new XElement((XName) "servers");
              foreach (Form1.Server server in this.m_Server.Distinct<Form1.Server>())
                xelement.Add((object) new XElement((XName) "shard", new object[3]
                {
                  (object) new XAttribute((XName) "login", (object) server.IP),
                  (object) new XAttribute((XName) "port", (object) server.Port),
                  (object) new XAttribute((XName) "lastselect", !(server.IP == this.serverList.Text) || !(server.Port.ToString() == this.port.Text) ? (object) "false" : (object) "true")
                }));
              xelement.Save(this.config_file);
              this.Dispose();
              process.WaitForExit();
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      this.m_Server = new List<Form1.Server>();
      RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\uopatch.exe");
      if (registryKey == null)
        this.txtUokrPath.Text = "Ultima Online Enhanced client not found!";
      else
        this.txtUokrPath.Text = registryKey.GetValue("").ToString().Replace("uopatch.exe", "");
      if (!File.Exists(this.config_file))
      {
        File.Create(this.config_file).Close();
      }
      else
      {
        XElement xelement = XElement.Load(this.config_file);
        int num = 0;
        this.m_Server.Clear();
        foreach (XElement element in xelement.Elements((XName) "shard"))
        {
          this.m_Server.Add(new Form1.Server()
          {
            index = num,
            IP = element.Attribute((XName) "login").Value,
            Port = int.Parse(element.Attribute((XName) "port").Value),
            LastSelect = Convert.ToBoolean(element.Attribute((XName) "lastselect").Value)
          });
          ++num;
        }
        this.serverList.DataSource = (object) this.m_Server.Select<Form1.Server, string>((Func<Form1.Server, string>) (u => u.IP)).ToList<string>();
        this.serverList.SelectedIndex = this.serverList.Items.IndexOf((object) this.m_Server.Find((Predicate<Form1.Server>) (s => s.LastSelect)).IP);
      }
    }

    private void quit_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void serverList_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.serverList.SelectedItem == null)
        return;
      this.port.Text = this.m_Server.Find((Predicate<Form1.Server>) (s => s.index == this.serverList.SelectedIndex)).Port.ToString();
    }

    private uint ConvertIpAdress(IPAddress address)
    {
      byte[] addressBytes = address.GetAddressBytes();
      return ((uint) addressBytes[3] << 24) + ((uint) addressBytes[2] << 16) + ((uint) addressBytes[1] << 8) + (uint) addressBytes[0];
    }

    private ushort ConvertPort(int port)
    {
      byte[] bytes = BitConverter.GetBytes(port);
      return (ushort) ((uint) bytes[0] << 8 | (uint) bytes[1]);
    }

    private static void PathOrLaunch(int id)
    {
      int num1 = 0;
      int num2 = 0;
      int iVersion = 0;
      int num3 = 0;
      Form1.ENCRYPTION_PATCH_TYPE encryptionPatchType = Form1.ENCRYPTION_PATCH_TYPE.Both;
      Process process = (Process) null;
      Stream pc = (Stream) new ProcessStream((IntPtr) id);
      switch (encryptionPatchType)
      {
        case Form1.ENCRYPTION_PATCH_TYPE.Login:
        case Form1.ENCRYPTION_PATCH_TYPE.Both:
          for (iVersion = 0; iVersion < StaticData.UOKR_LOGDATA_VERSION; ++iVersion)
          {
            num2 = Utility.Search(pc, StaticData.GetLoginData(iVersion), false);
            if ((uint) num2 > 0U)
              break;
          }
          if (num2 == 0)
          {
            pc.Close();
            process.Kill();
            return;
          }
          break;
      }
      switch (encryptionPatchType)
      {
        case Form1.ENCRYPTION_PATCH_TYPE.Game:
        case Form1.ENCRYPTION_PATCH_TYPE.Both:
          num3 = Utility.Search(pc, StaticData.UOSA_GAMEDATA_Send, false);
          if ((uint) num3 > 0U)
          {
            num1 = Utility.Search(pc, StaticData.UOSA_GAMEDATA_Receive, false);
            if ((uint) num1 <= 0U)
            {
              pc.Close();
              process.Kill();
              return;
            }
            break;
          }
          pc.Close();
          process.Kill();
          return;
      }
      switch (encryptionPatchType)
      {
        case Form1.ENCRYPTION_PATCH_TYPE.Login:
        case Form1.ENCRYPTION_PATCH_TYPE.Both:
          byte[] patchedLoginData = StaticData.GetPatchedLoginData(iVersion);
          pc.Seek((long) num2, SeekOrigin.Begin);
          pc.Write(patchedLoginData, 0, patchedLoginData.Length);
          break;
      }
      switch (encryptionPatchType)
      {
        case Form1.ENCRYPTION_PATCH_TYPE.Game:
        case Form1.ENCRYPTION_PATCH_TYPE.Both:
          byte[] patchedGameDataSend = StaticData.GetSAPatchedGameData_Send();
          pc.Seek((long) num3, SeekOrigin.Begin);
          pc.Write(patchedGameDataSend, 0, patchedGameDataSend.Length);
          byte[] patchedGameDataReceive = StaticData.GetSAPatchedGameData_Receive();
          pc.Seek((long) num1, SeekOrigin.Begin);
          pc.Write(patchedGameDataReceive, 0, patchedGameDataReceive.Length);
          break;
      }
      pc.Close();
      Thread.Sleep(10);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("mailto:mutila@gmail.com");
    }

    private void lnkOpen_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (this.ofdUOKRClient.ShowDialog((IWin32Window) this) != DialogResult.OK)
        return;
      this.txtUokrPath.Text = this.ofdUOKRClient.FileName.Substring(0, this.ofdUOKRClient.FileName.LastIndexOf('\\'));
    }

    private void groupBox1_Paint(object sender, PaintEventArgs e)
    {
      Graphics graphics1 = e.Graphics;
      Pen pen1 = new Pen(Color.Black, 1f);
      graphics1.DrawLine(pen1, 0, 5, 0, e.ClipRectangle.Height - 2);
      graphics1.DrawLine(pen1, 0, 5, 5, 5);
      graphics1.DrawLine(pen1, 90, 5, e.ClipRectangle.Width - 2, 5);
      Graphics graphics2 = graphics1;
      Pen pen2 = pen1;
      Rectangle clipRectangle1 = e.ClipRectangle;
      int x1_1 = clipRectangle1.Width - 2;
      int y1_1 = 5;
      clipRectangle1 = e.ClipRectangle;
      int x2_1 = clipRectangle1.Width - 2;
      clipRectangle1 = e.ClipRectangle;
      int y2_1 = clipRectangle1.Height - 2;
      graphics2.DrawLine(pen2, x1_1, y1_1, x2_1, y2_1);
      Graphics graphics3 = graphics1;
      Pen pen3 = pen1;
      Rectangle clipRectangle2 = e.ClipRectangle;
      int x1_2 = clipRectangle2.Width - 2;
      clipRectangle2 = e.ClipRectangle;
      int y1_2 = clipRectangle2.Height - 2;
      int x2_2 = 0;
      clipRectangle2 = e.ClipRectangle;
      int y2_2 = clipRectangle2.Height - 2;
      graphics3.DrawLine(pen3, x1_2, y1_2, x2_2, y2_2);
    }

    private void groupBox2_Paint(object sender, PaintEventArgs e)
    {
      Graphics graphics1 = e.Graphics;
      Pen pen1 = new Pen(Color.Black, 1f);
      graphics1.DrawLine(pen1, 0, 5, 0, e.ClipRectangle.Height - 2);
      graphics1.DrawLine(pen1, 0, 5, 5, 5);
      graphics1.DrawLine(pen1, 45, 5, e.ClipRectangle.Width - 2, 5);
      Graphics graphics2 = graphics1;
      Pen pen2 = pen1;
      Rectangle clipRectangle1 = e.ClipRectangle;
      int x1_1 = clipRectangle1.Width - 2;
      int y1_1 = 5;
      clipRectangle1 = e.ClipRectangle;
      int x2_1 = clipRectangle1.Width - 2;
      clipRectangle1 = e.ClipRectangle;
      int y2_1 = clipRectangle1.Height - 2;
      graphics2.DrawLine(pen2, x1_1, y1_1, x2_1, y2_1);
      Graphics graphics3 = graphics1;
      Pen pen3 = pen1;
      Rectangle clipRectangle2 = e.ClipRectangle;
      int x1_2 = clipRectangle2.Width - 2;
      clipRectangle2 = e.ClipRectangle;
      int y1_2 = clipRectangle2.Height - 2;
      int x2_2 = 0;
      clipRectangle2 = e.ClipRectangle;
      int y2_2 = clipRectangle2.Height - 2;
      graphics3.DrawLine(pen3, x1_2, y1_2, x2_2, y2_2);
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      Process.Start("github.com/Mutilador/UOELauncher");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.quit = new System.Windows.Forms.Button();
            this.okay = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.port = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.serverList = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblClient = new System.Windows.Forms.Label();
            this.lnkOpen = new System.Windows.Forms.LinkLabel();
            this.txtUokrPath = new System.Windows.Forms.TextBox();
            this.ofdUOKRClient = new System.Windows.Forms.OpenFileDialog();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // quit
            // 
            this.quit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.quit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.quit.Location = new System.Drawing.Point(269, 194);
            this.quit.Name = "quit";
            this.quit.Size = new System.Drawing.Size(72, 24);
            this.quit.TabIndex = 24;
            this.quit.Text = "&Quit";
            this.quit.Click += new System.EventHandler(this.quit_Click);
            // 
            // okay
            // 
            this.okay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okay.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.okay.Location = new System.Drawing.Point(182, 194);
            this.okay.Name = "okay";
            this.okay.Size = new System.Drawing.Size(72, 24);
            this.okay.TabIndex = 23;
            this.okay.Text = "&Play";
            this.okay.Click += new System.EventHandler(this.okay_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.port);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.serverList);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.groupBox2.Location = new System.Drawing.Point(12, 114);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(501, 64);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Shard";
            this.groupBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBox2_Paint);
            // 
            // port
            // 
            this.port.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.port.Location = new System.Drawing.Point(343, 24);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(40, 20);
            this.port.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(55, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Server:";
            // 
            // serverList
            // 
            this.serverList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.serverList.Location = new System.Drawing.Point(99, 24);
            this.serverList.Name = "serverList";
            this.serverList.Size = new System.Drawing.Size(196, 21);
            this.serverList.TabIndex = 11;
            this.serverList.SelectedIndexChanged += new System.EventHandler(this.serverList_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(307, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 16);
            this.label4.TabIndex = 12;
            this.label4.Text = "Port:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.linkLabel1.LinkColor = System.Drawing.Color.DarkRed;
            this.linkLabel1.Location = new System.Drawing.Point(148, 235);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(256, 13);
            this.linkLabel1.TabIndex = 25;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://github.com/Mutilador/UOELauncher";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Red;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblClient);
            this.groupBox1.Controls.Add(this.lnkOpen);
            this.groupBox1.Controls.Add(this.txtUokrPath);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(501, 100);
            this.groupBox1.TabIndex = 103;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Client Options";
            this.groupBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBox1_Paint);
            // 
            // lblClient
            // 
            this.lblClient.AutoSize = true;
            this.lblClient.Location = new System.Drawing.Point(14, 35);
            this.lblClient.Name = "lblClient";
            this.lblClient.Size = new System.Drawing.Size(118, 13);
            this.lblClient.TabIndex = 104;
            this.lblClient.Text = "UO:SA Client path: ";
            // 
            // lnkOpen
            // 
            this.lnkOpen.AutoSize = true;
            this.lnkOpen.LinkColor = System.Drawing.Color.ForestGreen;
            this.lnkOpen.Location = new System.Drawing.Point(352, 35);
            this.lnkOpen.Name = "lnkOpen";
            this.lnkOpen.Size = new System.Drawing.Size(110, 13);
            this.lnkOpen.TabIndex = 103;
            this.lnkOpen.TabStop = true;
            this.lnkOpen.Text = "Select a new path";
            this.lnkOpen.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpen_LinkClicked);
            // 
            // txtUokrPath
            // 
            this.txtUokrPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUokrPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtUokrPath.Location = new System.Drawing.Point(17, 51);
            this.txtUokrPath.Name = "txtUokrPath";
            this.txtUokrPath.ReadOnly = true;
            this.txtUokrPath.Size = new System.Drawing.Size(445, 20);
            this.txtUokrPath.TabIndex = 102;
            this.txtUokrPath.TabStop = false;
            this.txtUokrPath.WordWrap = false;
            // 
            // ofdUOKRClient
            // 
            this.ofdUOKRClient.Filter = "UO:SA Client|uosa.exe";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(525, 258);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.quit);
            this.Controls.Add(this.okay);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UO Enhanced Client Launcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    public class Server
    {
      public int index;

      public string IP { get; set; }

      public int Port { get; set; }

      public bool LastSelect { get; set; }
    }

    private enum ENCRYPTION_PATCH_TYPE
    {
      None,
      Login,
      Game,
      Both,
      End,
    }
  }
}
