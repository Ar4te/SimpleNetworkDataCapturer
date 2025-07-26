using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleNetworkDataCapturer.Lib.Models;

namespace SimpleNetworkDataCapturer.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NetworkCaptureDemo.PacketCaptured += OnPacketCaptured;
        NetworkCaptureDemo.ErrorOccurred += OnErrorOccurred;
    }

    private void OnPacketCaptured(object? sender, NetworkPacket packet)
    {
        // 可选：输出到控制台
        System.Diagnostics.Debug.WriteLine($"捕获到数据包: {packet.DisplayInfo}");
    }

    private void OnErrorOccurred(object? sender, string error)
    {
        MessageBox.Show($"抓包错误: {error}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}