using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SimpleNetworkDataCapturer.Models;
using SimpleNetworkDataCapturer.Services;

namespace SimpleNetworkDataCapturer.ViewModels;

/// <summary>
/// 网络抓包ViewModel
/// </summary>
public class NetworkCaptureViewModel : INotifyPropertyChanged
{
    private readonly NetworkCaptureService _captureService;
    private readonly PacketFilterService _filterService;
    private bool _isCapturing;
    private string _statusMessage = "就绪";
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

    public NetworkCaptureViewModel()
    {
        _captureService = new NetworkCaptureService();
        _captureService.PacketCaptured += OnPacketCaptured;
        _captureService.ErrorOccurred += OnErrorOccurred;

        _filterService = new PacketFilterService();
        Packets = new ObservableCollection<NetworkPacket>();
        AvailableDevices = _captureService.GetAvailableDevices();

        StartCaptureCommand = new RelayCommand(StartCapture, CanStartCapture);
        StopCaptureCommand = new RelayCommand(StopCapture, CanStopCapture);
        ClearCommand = new RelayCommand(ClearPackets, CanClearPackets);
        ToggleFilterCommand = new RelayCommand(ToggleFilter, CanToggleFilter);
    }

    /// <summary>
    /// 数据包列表
    /// </summary>
    public ObservableCollection<NetworkPacket> Packets { get; }

    /// <summary>
    /// 可用设备列表
    /// </summary>
    public List<string> AvailableDevices { get; }

    /// <summary>
    /// 是否正在抓包
    /// </summary>
    public bool IsCapturing
    {
        get => _isCapturing;
        set
        {
            _isCapturing = value;
            OnPropertyChanged();
            ((RelayCommand)StartCaptureCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopCaptureCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 总数据包数
    /// </summary>
    public int TotalPackets
    {
        get => _totalPackets;
        set
        {
            _totalPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// TCP数据包数
    /// </summary>
    public int TcpPackets
    {
        get => _tcpPackets;
        set
        {
            _tcpPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// UDP数据包数
    /// </summary>
    public int UdpPackets
    {
        get => _udpPackets;
        set
        {
            _udpPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// HTTP数据包数
    /// </summary>
    public int HttpPackets
    {
        get => _httpPackets;
        set
        {
            _httpPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// HTTPS数据包数
    /// </summary>
    public int HttpsPackets
    {
        get => _httpsPackets;
        set
        {
            _httpsPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// DNS数据包数
    /// </summary>
    public int DnsPackets
    {
        get => _dnsPackets;
        set
        {
            _dnsPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// DHCP数据包数
    /// </summary>
    public int DhcpPackets
    {
        get => _dhcpPackets;
        set
        {
            _dhcpPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ICMP数据包数
    /// </summary>
    public int IcmpPackets
    {
        get => _icmpPackets;
        set
        {
            _icmpPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Ethernet数据包数
    /// </summary>
    public int EthernetPackets
    {
        get => _ethernetPackets;
        set
        {
            _ethernetPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 其他协议数据包数
    /// </summary>
    public int OtherPackets
    {
        get => _otherPackets;
        set
        {
            _otherPackets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否启用过滤
    /// </summary>
    public bool IsFilterEnabled
    {
        get => _filterService.IsFilterEnabled;
        set
        {
            _filterService.IsFilterEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 开始抓包命令
    /// </summary>
    public ICommand StartCaptureCommand { get; }

    /// <summary>
    /// 停止抓包命令
    /// </summary>
    public ICommand StopCaptureCommand { get; }

    /// <summary>
    /// 清空数据包命令
    /// </summary>
    public ICommand ClearCommand { get; }

    /// <summary>
    /// 切换过滤命令
    /// </summary>
    public ICommand ToggleFilterCommand { get; }

    /// <summary>
    /// 开始抓包
    /// </summary>
    private async void StartCapture()
    {
        try
        {
            IsCapturing = true;
            StatusMessage = "正在启动抓包...";
            
            await _captureService.StartCaptureAsync().ConfigureAwait(false);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = "抓包已启动";
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"启动失败: {ex.Message}";
                IsCapturing = false;
            });
        }
    }

    /// <summary>
    /// 停止抓包
    /// </summary>
    private void StopCapture()
    {
        try
        {
            _captureService.StopCapture();
            IsCapturing = false;
            StatusMessage = "抓包已停止";
        }
        catch (Exception ex)
        {
            StatusMessage = $"停止失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 清空数据包
    /// </summary>
    private void ClearPackets()
    {
        Packets.Clear();
        TotalPackets = 0;
        TcpPackets = 0;
        UdpPackets = 0;
        HttpPackets = 0;
        HttpsPackets = 0;
        DnsPackets = 0;
        DhcpPackets = 0;
        IcmpPackets = 0;
        EthernetPackets = 0;
        OtherPackets = 0;
        StatusMessage = "数据已清空";
    }

    /// <summary>
    /// 切换过滤
    /// </summary>
    private void ToggleFilter()
    {
        IsFilterEnabled = !IsFilterEnabled;
        StatusMessage = IsFilterEnabled ? "过滤已启用" : "过滤已禁用";
    }

    /// <summary>
    /// 是否可以开始抓包
    /// </summary>
    private bool CanStartCapture() => !IsCapturing;

    /// <summary>
    /// 是否可以停止抓包
    /// </summary>
    private bool CanStopCapture() => IsCapturing;

    /// <summary>
    /// 是否可以清空数据包
    /// </summary>
    private bool CanClearPackets() => Packets.Count > 0;

    /// <summary>
    /// 是否可以切换过滤
    /// </summary>
    private bool CanToggleFilter() => true;

    /// <summary>
    /// 数据包到达事件处理
    /// </summary>
    private void OnPacketCaptured(object? sender, NetworkPacket packet)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 应用过滤
            if (!_filterService.IsPacketPassed(packet))
            {
                return; // 过滤掉不匹配的数据包
            }

            Packets.Insert(0, packet); // 新数据包插入到顶部
            
            // 限制显示的数据包数量
            while (Packets.Count > 1000)
            {
                Packets.RemoveAt(Packets.Count - 1);
            }

            TotalPackets++;
            
            // 更新详细协议统计
            switch (packet.Protocol.ToUpper())
            {
                case "HTTP":
                    HttpPackets++;
                    TcpPackets++;
                    break;
                case "HTTPS":
                    HttpsPackets++;
                    TcpPackets++;
                    break;
                case "DNS":
                    DnsPackets++;
                    UdpPackets++;
                    break;
                case "DHCP":
                    DhcpPackets++;
                    UdpPackets++;
                    break;
                case "ICMP":
                    IcmpPackets++;
                    break;
                case "ETHERNET":
                    EthernetPackets++;
                    break;
                case "TCP":
                    TcpPackets++;
                    break;
                case "UDP":
                    UdpPackets++;
                    break;
                default:
                    OtherPackets++;
                    break;
            }
        });
    }

    /// <summary>
    /// 错误事件处理
    /// </summary>
    private void OnErrorOccurred(object? sender, string error)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"错误: {error}";
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _captureService?.Dispose();
    }
}

/// <summary>
/// 简单的中继命令实现
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
} 