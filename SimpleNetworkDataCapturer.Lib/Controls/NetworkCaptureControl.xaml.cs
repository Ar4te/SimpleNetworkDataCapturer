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
    private readonly PacketFilterService _filterService;
    private readonly ObservableCollection<NetworkPacket> _packets;
    
    private int _totalPackets;
    private int _tcpPackets;
    private int _udpPackets;
    private int _httpPackets;
    private int _httpsPackets;
    private int _dnsPackets;
    private int _dhcpPackets;
    private int _icmpPackets;
    private int _ethernetPackets;
    private int _otherPackets;
    private bool _isCapturing;

    public NetworkCaptureControl()
    {
        InitializeComponent();
        
        _captureService = new NetworkCaptureService();
        _captureService.PacketCaptured += OnPacketCaptured;
        _captureService.ErrorOccurred += OnErrorOccurred;
        
        _filterService = new PacketFilterService();
        _packets = new ObservableCollection<NetworkPacket>();
        PacketsDataGrid.ItemsSource = _packets;
        
        // 加载保存的过滤规则
        _ = LoadFilterRulesAsync();
        
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
        _httpPackets = 0;
        _httpsPackets = 0;
        _dnsPackets = 0;
        _dhcpPackets = 0;
        _icmpPackets = 0;
        _ethernetPackets = 0;
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
    public (int Total, int Tcp, int Udp, int Http, int Https, int Dns, int Dhcp, int Icmp, int Other) GetStatistics()
    {
        return (_totalPackets, _tcpPackets, _udpPackets, _httpPackets, _httpsPackets, _dnsPackets, _dhcpPackets, _icmpPackets, _otherPackets);
    }

    /// <summary>
    /// 是否正在抓包
    /// </summary>
    public bool IsCapturing => _isCapturing;

    /// <summary>
    /// 获取过滤服务
    /// </summary>
    public PacketFilterService FilterService => _filterService;

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

    private void FilterCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _filterService.IsFilterEnabled = true;
        StatusText.Text = "过滤已启用";
    }

    private void FilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        _filterService.IsFilterEnabled = false;
        StatusText.Text = "过滤已禁用";
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFilterDialog();
    }

    private void OnPacketCaptured(object? sender, NetworkPacket packet)
    {
        Dispatcher.Invoke(() =>
        {
            // 应用过滤
            if (!_filterService.IsPacketPassed(packet))
            {
                return; // 过滤掉不匹配的数据包
            }

            _packets.Insert(0, packet); // 新数据包插入到顶部
            
            // 限制显示的数据包数量
            while (_packets.Count > 1000)
            {
                _packets.RemoveAt(_packets.Count - 1);
            }

            _totalPackets++;
            
            // 更新详细协议统计
            switch (packet.Protocol.ToUpper())
            {
                case "HTTP":
                    _httpPackets++;
                    _tcpPackets++;
                    break;
                case "HTTPS":
                    _httpsPackets++;
                    _tcpPackets++;
                    break;
                case "DNS":
                    _dnsPackets++;
                    _udpPackets++;
                    break;
                case "DHCP":
                    _dhcpPackets++;
                    _udpPackets++;
                    break;
                case "ICMP":
                    _icmpPackets++;
                    break;
                case "ETHERNET":
                    _ethernetPackets++;
                    break;
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
        HttpText.Text = _httpPackets.ToString();
        HttpsText.Text = _httpsPackets.ToString();
        UdpText.Text = _udpPackets.ToString();
        DnsText.Text = _dnsPackets.ToString();
        DhcpText.Text = _dhcpPackets.ToString();
        IcmpText.Text = _icmpPackets.ToString();
        OtherText.Text = _otherPackets.ToString();
        UpdateButtonStates();
    }

    public void Dispose()
    {
        _captureService?.Dispose();
    }

    /// <summary>
    /// 显示过滤设置对话框
    /// </summary>
    private void ShowFilterDialog()
    {
        var filterDialog = new FilterDialog(_filterService);
        filterDialog.Owner = Window.GetWindow(this);
        filterDialog.ShowDialog();
    }
    
    /// <summary>
    /// 加载过滤规则
    /// </summary>
    private async Task LoadFilterRulesAsync()
    {
        try
        {
            await _filterService.LoadFilterRulesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载过滤规则失败: {ex.Message}");
        }
    }
} 