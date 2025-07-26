using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SimpleNetworkDataCapturer.Models;
using SimpleNetworkDataCapturer.Services;

namespace SimpleNetworkDataCapturer.Controls;

/// <summary>
/// 网络抓包控件
/// </summary>
public partial class NetworkCaptureControl : UserControl, IDisposable
{
    private readonly NetworkCaptureService _captureService;
    private readonly ObservableCollection<NetworkPacket> _packets;
    
    private int _totalPackets;
    private int _tcpPackets;
    private int _udpPackets;
    private int _otherPackets;
    private bool _isCapturing;

    public NetworkCaptureControl()
    {
        InitializeComponent();
        
        _captureService = new NetworkCaptureService();
        _captureService.PacketCaptured += OnPacketCaptured;
        _captureService.ErrorOccurred += OnErrorOccurred;
        
        _packets = new ObservableCollection<NetworkPacket>();
        PacketsDataGrid.ItemsSource = _packets;
        
        UpdateButtonStates();
    }

    /// <summary>
    /// 数据包到达事件
    /// </summary>
    public event EventHandler<NetworkPacket>? PacketCaptured;

    /// <summary>
    /// 错误事件
    /// </summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// 开始抓包
    /// </summary>
    public async Task StartCaptureAsync()
    {
        if (_isCapturing) return;

        try
        {
            _isCapturing = true;
            UpdateButtonStates();
            StatusText.Text = "正在启动抓包...";
            
            await _captureService.StartCaptureAsync();
            
            StatusText.Text = "抓包已启动";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"启动失败: {ex.Message}";
            _isCapturing = false;
            UpdateButtonStates();
            ErrorOccurred?.Invoke(this, ex.Message);
        }
    }

    /// <summary>
    /// 停止抓包
    /// </summary>
    public void StopCapture()
    {
        if (!_isCapturing) return;

        try
        {
            _captureService.StopCapture();
            _isCapturing = false;
            UpdateButtonStates();
            StatusText.Text = "抓包已停止";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"停止失败: {ex.Message}";
            ErrorOccurred?.Invoke(this, ex.Message);
        }
    }

    /// <summary>
    /// 清空数据包
    /// </summary>
    public void ClearPackets()
    {
        _packets.Clear();
        _totalPackets = 0;
        _tcpPackets = 0;
        _udpPackets = 0;
        _otherPackets = 0;
        UpdateStatistics();
        StatusText.Text = "数据已清空";
    }

    /// <summary>
    /// 获取当前数据包列表
    /// </summary>
    public IReadOnlyCollection<NetworkPacket> GetPackets()
    {
        return _packets.ToList().AsReadOnly();
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public (int Total, int Tcp, int Udp, int Other) GetStatistics()
    {
        return (_totalPackets, _tcpPackets, _udpPackets, _otherPackets);
    }

    /// <summary>
    /// 是否正在抓包
    /// </summary>
    public bool IsCapturing => _isCapturing;

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        _ = StartCaptureAsync();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopCapture();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearPackets();
    }

    private void OnPacketCaptured(object? sender, NetworkPacket packet)
    {
        Dispatcher.Invoke(() =>
        {
            _packets.Insert(0, packet); // 新数据包插入到顶部
            
            // 限制显示的数据包数量
            while (_packets.Count > 1000)
            {
                _packets.RemoveAt(_packets.Count - 1);
            }

            _totalPackets++;
            
            // 更新协议统计
            switch (packet.Protocol.ToUpper())
            {
                case "TCP":
                    _tcpPackets++;
                    break;
                case "UDP":
                    _udpPackets++;
                    break;
                default:
                    _otherPackets++;
                    break;
            }

            UpdateStatistics();
            PacketCaptured?.Invoke(this, packet);
        });
    }

    private void OnErrorOccurred(object? sender, string error)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = $"错误: {error}";
            ErrorOccurred?.Invoke(this, error);
        });
    }

    private void UpdateButtonStates()
    {
        StartButton.IsEnabled = !_isCapturing;
        StopButton.IsEnabled = _isCapturing;
        ClearButton.IsEnabled = _packets.Count > 0;
    }

    private void UpdateStatistics()
    {
        TotalText.Text = _totalPackets.ToString();
        TcpText.Text = _tcpPackets.ToString();
        UdpText.Text = _udpPackets.ToString();
        OtherText.Text = _otherPackets.ToString();
        UpdateButtonStates();
    }

    public void Dispose()
    {
        _captureService?.Dispose();
    }
} 