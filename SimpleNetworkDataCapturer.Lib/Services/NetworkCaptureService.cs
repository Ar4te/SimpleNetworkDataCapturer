using SharpPcap;
using PacketDotNet;
using SimpleNetworkDataCapturer.Models;
using System.Collections.Concurrent;

namespace SimpleNetworkDataCapturer.Services;

/// <summary>
/// 网络抓包服务
/// </summary>
public class NetworkCaptureService : IDisposable
{
    private readonly ConcurrentQueue<NetworkPacket> _packetQueue = new();
    private readonly List<ILiveDevice> _devices = new();
    private readonly object _lockObject = new();
    
    public event EventHandler<NetworkPacket>? PacketCaptured;
    public event EventHandler<string>? ErrorOccurred;
    
    private bool _isCapturing = false;
    private readonly int _maxQueueSize = 10000;

    /// <summary>
    /// 获取所有可用网卡
    /// </summary>
    public List<string> GetAvailableDevices()
    {
        var devices = new List<string>();
        try
        {
            var deviceList = CaptureDeviceList.Instance;
            foreach (var device in deviceList)
            {
                devices.Add($"{device.Name} - {device.Description}");
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"获取网卡列表失败: {ex.Message}");
        }
        return devices;
    }

    /// <summary>
    /// 开始抓包
    /// </summary>
    public async Task StartCaptureAsync()
    {
        if (_isCapturing) return;

        try
        {
            _isCapturing = true;
            var deviceList = CaptureDeviceList.Instance;
            
            foreach (var device in deviceList)
            {
                try
                {
                    // 打开设备
                    device.Open();
                    
                    // 设置过滤器以捕获所有流量
                    device.Filter = "";
                    
                    // 注册数据包到达事件
                    device.OnPacketArrival += OnPacketArrival;
                    
                    // 开始捕获
                    device.StartCapture();
                    
                    _devices.Add(device);
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, $"启动设备 {device.Name} 失败: {ex.Message}");
                }
            }

            // 启动后台任务处理队列
            _ = Task.Run(ProcessPacketQueue);
        }
        catch (Exception ex)
        {
            _isCapturing = false;
            ErrorOccurred?.Invoke(this, $"启动抓包失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 停止抓包
    /// </summary>
    public void StopCapture()
    {
        if (!_isCapturing) return;

        lock (_lockObject)
        {
            _isCapturing = false;
            
            foreach (var device in _devices)
            {
                try
                {
                    device.StopCapture();
                    device.Close();
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(this, $"停止设备 {device.Name} 失败: {ex.Message}");
                }
            }
            
            _devices.Clear();
        }
    }

    /// <summary>
    /// 数据包到达事件处理
    /// </summary>
    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var packet = ParsePacket(e);
            if (packet != null)
            {
                _packetQueue.Enqueue(packet);
                
                // 限制队列大小
                while (_packetQueue.Count > _maxQueueSize)
                {
                    _packetQueue.TryDequeue(out _);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"解析数据包失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理数据包队列
    /// </summary>
    private async Task ProcessPacketQueue()
    {
        while (_isCapturing)
        {
            if (_packetQueue.TryDequeue(out var packet))
            {
                PacketCaptured?.Invoke(this, packet);
            }
            else
            {
                await Task.Delay(10); // 短暂延迟避免CPU占用过高
            }
        }
    }

    /// <summary>
    /// 解析数据包
    /// </summary>
    private NetworkPacket? ParsePacket(PacketCapture capture)
    {
        try
        {
            var rawPacket = capture.GetPacket();
            
            // 安全检查数据包长度
            if (rawPacket.Data.Length < 14) // 最小以太网帧长度
            {
                return null;
            }
            
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
            
            var networkPacket = new NetworkPacket
            {
                CaptureTime = rawPacket.Timeval.Date,
                Length = rawPacket.Data.Length,
                RawData = rawPacket.Data
            };

            // 解析IP层
            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket != null)
            {
                networkPacket.SourceAddress = ipPacket.SourceAddress.ToString();
                networkPacket.DestinationAddress = ipPacket.DestinationAddress.ToString();
                networkPacket.Protocol = ipPacket.Protocol.ToString();

                // 解析传输层
                switch (ipPacket.Protocol)
                {
                    case ProtocolType.Tcp:
                        var tcpPacket = packet.Extract<TcpPacket>();
                        if (tcpPacket != null)
                        {
                            networkPacket.SourcePort = tcpPacket.SourcePort;
                            networkPacket.DestinationPort = tcpPacket.DestinationPort;
                            
                            // 识别HTTP/HTTPS协议
                            var protocol = IdentifyApplicationProtocol(tcpPacket);
                            networkPacket.Protocol = protocol;
                            
                            // 提取TCP负载数据
                            if (tcpPacket.PayloadData.Length > 0)
                            {
                                networkPacket.RawData = tcpPacket.PayloadData;
                            }
                        }
                        break;

                    case ProtocolType.Udp:
                        var udpPacket = packet.Extract<UdpPacket>();
                        if (udpPacket != null)
                        {
                            networkPacket.SourcePort = udpPacket.SourcePort;
                            networkPacket.DestinationPort = udpPacket.DestinationPort;
                            
                            // 识别DNS等UDP协议
                            var protocol = IdentifyUdpProtocol(udpPacket);
                            networkPacket.Protocol = protocol;
                            
                            // 提取UDP负载数据
                            if (udpPacket.PayloadData.Length > 0)
                            {
                                networkPacket.RawData = udpPacket.PayloadData;
                            }
                        }
                        break;

                    case ProtocolType.Icmp:
                        networkPacket.Protocol = "ICMP";
                        break;

                    default:
                        networkPacket.Protocol = ipPacket.Protocol.ToString();
                        break;
                }
            }
            else
            {
                // 非IP包，可能是ARP等
                var ethernetPacket = packet.Extract<EthernetPacket>();
                if (ethernetPacket != null)
                {
                    networkPacket.Protocol = "Ethernet";
                    networkPacket.SourceAddress = ethernetPacket.SourceHardwareAddress.ToString();
                    networkPacket.DestinationAddress = ethernetPacket.DestinationHardwareAddress.ToString();
                }
                else
                {
                    networkPacket.Protocol = "Other";
                }
            }

            return networkPacket;
        }
        catch (Exception ex)
        {
            // 不抛出错误，只记录日志，避免影响其他数据包处理
            System.Diagnostics.Debug.WriteLine($"解析数据包失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 识别TCP应用层协议
    /// </summary>
    private string IdentifyApplicationProtocol(TcpPacket tcpPacket)
    {
        if (tcpPacket.PayloadData.Length == 0) return "TCP";

        try
        {
            var payload = tcpPacket.PayloadData;
            if (payload.Length < 4) return "TCP";

            // 检查HTTP
            if (payload.Length >= 4)
            {
                var header = System.Text.Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 20));
                if (header.StartsWith("GET ") || header.StartsWith("POST ") || 
                    header.StartsWith("PUT ") || header.StartsWith("DELETE ") ||
                    header.StartsWith("HEAD ") || header.StartsWith("OPTIONS "))
                {
                    return "HTTP";
                }
                if (header.StartsWith("HTTP/"))
                {
                    return "HTTP";
                }
            }

            // 检查HTTPS (TLS)
            if (payload.Length >= 1)
            {
                if (payload[0] == 0x16 || payload[0] == 0x17 || payload[0] == 0x15) // TLS握手
                {
                    return "HTTPS";
                }
            }

            // 检查FTP
            if (payload.Length >= 3)
            {
                var header = System.Text.Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 10));
                if (header.StartsWith("220 ") || header.StartsWith("USER ") || 
                    header.StartsWith("PASS ") || header.StartsWith("QUIT "))
                {
                    return "FTP";
                }
            }

            // 检查SMTP
            if (payload.Length >= 4)
            {
                var header = System.Text.Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 10));
                if (header.StartsWith("220 ") || header.StartsWith("EHLO ") || 
                    header.StartsWith("HELO ") || header.StartsWith("MAIL "))
                {
                    return "SMTP";
                }
            }

            // 检查POP3
            if (payload.Length >= 4)
            {
                var header = System.Text.Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 10));
                if (header.StartsWith("+OK") || header.StartsWith("USER ") || 
                    header.StartsWith("PASS ") || header.StartsWith("QUIT "))
                {
                    return "POP3";
                }
            }

            // 检查IMAP
            if (payload.Length >= 4)
            {
                var header = System.Text.Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 10));
                if (header.StartsWith("* OK") || header.StartsWith("a001 ") || 
                    header.StartsWith("A001 ") || header.StartsWith("a001 LOGIN"))
                {
                    return "IMAP";
                }
            }

            return "TCP";
        }
        catch
        {
            return "TCP";
        }
    }

    /// <summary>
    /// 识别UDP应用层协议
    /// </summary>
    private string IdentifyUdpProtocol(UdpPacket udpPacket)
    {
        // DNS
        if (udpPacket.SourcePort == 53 || udpPacket.DestinationPort == 53)
        {
            return "DNS";
        }

        // DHCP
        if (udpPacket.SourcePort == 67 || udpPacket.DestinationPort == 67 ||
            udpPacket.SourcePort == 68 || udpPacket.DestinationPort == 68)
        {
            return "DHCP";
        }

        // SNMP
        if (udpPacket.SourcePort == 161 || udpPacket.DestinationPort == 161 ||
            udpPacket.SourcePort == 162 || udpPacket.DestinationPort == 162)
        {
            return "SNMP";
        }

        // NTP
        if (udpPacket.SourcePort == 123 || udpPacket.DestinationPort == 123)
        {
            return "NTP";
        }

        return "UDP";
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopCapture();
        GC.SuppressFinalize(this);
    }
} 