﻿using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FadeCandySharpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer m_serverExeCheck;   

        public MainWindow()
        {
            InitializeComponent();

            m_serverExeCheck = new Timer();
            m_serverExeCheck.AutoReset = true;
            m_serverExeCheck.Elapsed += CheckServerExe;
            m_serverExeCheck.Interval = 500;
            m_serverExeCheck.Enabled = true;
        }

        void CheckServerExe(object sender, ElapsedEventArgs e)
        {
            Process[] processlist = Process.GetProcesses();

            bool serverFound = false;
            foreach(Process process in processlist)
            {
                if(process.ProcessName.ToLower().Equals("fcserver"))
                {
                    serverFound = true;
                    break;
                }
            }

            ServerWarn.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, 
                new Action(delegate()
            {
                ServerWarn.Visibility = serverFound ? Visibility.Collapsed : Visibility.Visible;
            }));
        }

        private void FadeCandySharpSDK_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/QuinnDamerell/FadeCandySharp");
        }

        private void FadeCandySdk_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/scanlime/fadecandy/releases");
        }

        private void SimpleSampleButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleSample simpleSample = new SimpleSample(this);
            simpleSample.Show();
            simpleSample.Unloaded += UnblockButtons;
            BlockButtons();
        }

        private void BlockButtons()
        {
            SimpleSampleButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
                {
                    SimpleSampleButton.IsEnabled = false;
                }));
        }

        void UnblockButtons(object sender, RoutedEventArgs e)
        {
            SimpleSampleButton.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
               new Action(delegate()
               {
                   SimpleSampleButton.IsEnabled = true;
               }));
        }
    }
}
