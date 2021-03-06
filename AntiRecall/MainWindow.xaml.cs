﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using socks5;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using AntiRecall.deploy;

namespace AntiRecall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {

        private string port;
        private static NotifyIcon ni;
        public static socks5.Socks5Server proxy;
        public static double count { get; set; }
        public static bool is_recallmodule_load { get; set; }

        private void init_minimize()
        {
            MenuItem menuItem1 = new MenuItem();
            ContextMenu contextMenu = new ContextMenu();

            menuItem1.Index = 0;
            menuItem1.Text = "退出";
            menuItem1.Click += new System.EventHandler(menuItem1_Click);
            contextMenu.MenuItems.Add(menuItem1);

            ni = new NotifyIcon();
            ni.Text = "一个优雅的防撤回工具";
            ni.ContextMenu = contextMenu;
            ni.Visible = true;
#if DEBUG
            ni.Icon = new Icon("../../Resources/main.ico");
#else
            string truPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
         
            ni.Icon = new Icon(truPath + "\\Resources\\main.ico");
            
#endif
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {

                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private void init_socks5()
        {
            if (port!=null)
                proxy = new Socks5Server(IPAddress.Any, Convert.ToInt32(port));
            proxy.PacketSize = 65535;
            proxy.Start();
        }

        private delegate void TextChanger();

        private void UpdateCount()
        {
            Regex re = new Regex("\\[\\d*\\]");
#if DEBUG
            Console.WriteLine(count);
#endif
            Recall_Text.Text = re.Replace(Recall_Text.Text, "["+Convert.ToString(Math.Ceiling(count / 8))+"]");
        }

        public void ModifyRecallCount()
        {
            Recall_Text.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new TextChanger(UpdateCount));
        }

        
        public MainWindow()
        {
            InitializeComponent();
            ShortCut.init_shortcut("AntiRecall");
            init_minimize();
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // CoolButton Clicked! Let's show our InputBox.
            //InputBox.Visibility = System.Windows.Visibility.Visible;
            port = PortText.Text;
            Start.IsEnabled = false;
            Start.Content = "正在监听";
            init_socks5();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            ni.BalloonTipTitle = "AntiRecall";
            ni.BalloonTipText = "已将AntiRecall最小化到托盘,程序将在后台运行";
            ni.BalloonTipIcon = ToolTipIcon.Info;
            ni.ShowBalloonTip(30000);
            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ni.Visible = false;
            if (proxy!=null)
                proxy.Stop();
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            ni.Visible = false;
            if (proxy != null)
                proxy.Stop();
            Close();
        }
    }
}
