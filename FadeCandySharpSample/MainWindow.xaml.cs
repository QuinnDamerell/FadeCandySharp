using FadeCandySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        FadeCandyDeviceFactory m_factory;

        public MainWindow()
        {
            InitializeComponent();

            m_factory = new FadeCandyDeviceFactory("127.0.0.1", 7890);
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            List<FadeCandyDevice> list =  m_factory.GetDevices();
            if(list.Count > 0)
            {
                list[0].StartDrawing();
            }
        }
    }
}
