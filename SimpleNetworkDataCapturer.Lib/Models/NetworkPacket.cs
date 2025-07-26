using System.Text;

namespace SimpleNetworkDataCapturer.Models;

/// <summary>
/// 网络数据包模型
/// </summary>
public class NetworkPacket
{
    /// <summary>
    /// 捕获时间
    /// </summary>
    public DateTime CaptureTime { get; set; }

    /// <summary>
    /// 源地址
    /// </summary>
    public string SourceAddress { get; set; } = string.Empty;

    /// <summary>
    /// 源端口
    /// </summary>
    public int SourcePort { get; set; }

    /// <summary>
    /// 目标地址
    /// </summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// 目标端口
    /// </summary>
    public int DestinationPort { get; set; }

    /// <summary>
    /// 协议类型
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// 数据包长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    public byte[] RawData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 可读字符串信息
    /// </summary>
    public string ReadableData => GetReadableData();

    /// <summary>
    /// 十六进制信息
    /// </summary>
    public string HexData => GetHexData();

    /// <summary>
    /// 格式化时间字符串
    /// </summary>
    public string FormattedTime => CaptureTime.ToString("yyyyMMdd HH:mm:ss:ffff");

    /// <summary>
    /// 获取可读字符串数据
    /// </summary>
    private string GetReadableData()
    {
        if (RawData.Length == 0) return string.Empty;

        var readable = new StringBuilder();
        foreach (var b in RawData)
        {
            if (b >= 32 && b <= 126) // 可打印ASCII字符
            {
                readable.Append((char)b);
            }
            else
            {
                readable.Append('.');
            }
        }
        return readable.ToString();
    }

    /// <summary>
    /// 获取十六进制数据
    /// </summary>
    private string GetHexData()
    {
        if (RawData.Length == 0) return string.Empty;

        return BitConverter.ToString(RawData).Replace("-", " ");
    }

    /// <summary>
    /// 获取简化的显示信息
    /// </summary>
    public string DisplayInfo => $"{FormattedTime} | {SourceAddress}:{SourcePort} -> {DestinationAddress}:{DestinationPort} | {Protocol} | {Length} bytes";
} 