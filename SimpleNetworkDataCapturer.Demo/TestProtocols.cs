using System.Text;
using PacketDotNet;

namespace SimpleNetworkDataCapturer.Demo;

/// <summary>
/// 协议识别测试类
/// </summary>
public static class TestProtocols
{
    /// <summary>
    /// 测试HTTP协议识别
    /// </summary>
    public static void TestHttpProtocol()
    {
        var httpRequest = "GET /index.html HTTP/1.1\r\nHost: example.com\r\n\r\n";
        var httpResponse = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n";
        
        var requestBytes = Encoding.ASCII.GetBytes(httpRequest);
        var responseBytes = Encoding.ASCII.GetBytes(httpResponse);
        
        Console.WriteLine($"HTTP Request: {IdentifyProtocol(requestBytes)}");
        Console.WriteLine($"HTTP Response: {IdentifyProtocol(responseBytes)}");
    }

    /// <summary>
    /// 测试HTTPS协议识别
    /// </summary>
    public static void TestHttpsProtocol()
    {
        // TLS Client Hello
        var tlsClientHello = new byte[] { 0x16, 0x03, 0x01, 0x00, 0x01, 0x01, 0x00 };
        
        // TLS Server Hello
        var tlsServerHello = new byte[] { 0x16, 0x03, 0x01, 0x00, 0x01, 0x02, 0x00 };
        
        Console.WriteLine($"TLS Client Hello: {IdentifyProtocol(tlsClientHello)}");
        Console.WriteLine($"TLS Server Hello: {IdentifyProtocol(tlsServerHello)}");
    }

    /// <summary>
    /// 识别协议类型
    /// </summary>
    private static string IdentifyProtocol(byte[] payload)
    {
        if (payload.Length == 0) return "Unknown";

        try
        {
            // 检查HTTP
            if (payload.Length >= 4)
            {
                var header = Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 20));
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
                if (payload[0] == 0x16 || payload[0] == 0x17 || payload[0] == 0x15)
                {
                    return "HTTPS";
                }
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
} 