# 网络数据包捕获器

基于.NET 8 和 SharpPcap 开发的网络抓包工具，支持 TCP、UDP、HTTP、HTTPS 等常见协议的捕获。

## 功能特性

- ✅ 捕获本机全部网卡的全部地址（包含 127.0.0.1 和 localhost）
- ✅ 支持全部端口的往返数据捕获
- ✅ 记录详细的时间信息（格式：yyyyMMdd HH:mm:ss:ffff）
- ✅ 显示源地址、源端口、目标地址、目标端口
- ✅ 智能识别协议类型（TCP、UDP、HTTP、HTTPS、DNS、DHCP、FTP、SMTP、POP3、IMAP 等）
- ✅ 提供可读字符串信息和十六进制信息
- ✅ 智能过滤功能，支持按地址、端口、协议、内容过滤
- ✅ 高级过滤规则组，支持复杂的规则组合关系（AND/OR/N-of-M）
- ✅ 详细协议统计，显示各种协议的包数量
- ✅ 简约大气的工业风 UI 设计
- ✅ 封装为 WPF 控件库，便于分发使用
- ✅ 强大的错误处理机制，避免单个数据包解析失败影响整体功能

## 支持的协议

### TCP 协议

- **HTTP** - 超文本传输协议
- **HTTPS** - 安全超文本传输协议（TLS/SSL）
- **FTP** - 文件传输协议
- **SMTP** - 简单邮件传输协议
- **POP3** - 邮局协议版本 3
- **IMAP** - 互联网消息访问协议

### UDP 协议

- **DNS** - 域名系统
- **DHCP** - 动态主机配置协议
- **SNMP** - 简单网络管理协议
- **NTP** - 网络时间协议

### 其他协议

- **ICMP** - 互联网控制消息协议
- **Ethernet** - 以太网帧

## 技术栈

- **.NET 8** - 最新的.NET 框架
- **WPF** - Windows Presentation Foundation 用户界面
- **SharpPcap** - 网络数据包捕获库
- **PacketDotNet** - 数据包解析库

## 项目结构

```
SimpleNetworkDataCapturer/
├── SimpleNetworkDataCapturer.sln              # 解决方案文件
├── SimpleNetworkDataCapturer.Lib/             # 核心库项目
│   ├── Models/
│   │   └── NetworkPacket.cs                   # 网络数据包模型
│   ├── Services/
│   │   └── NetworkCaptureService.cs           # 核心抓包服务
│   ├── ViewModels/
│   │   └── NetworkCaptureViewModel.cs         # MVVM视图模型
│   ├── Controls/
│   │   ├── NetworkCaptureControl.xaml         # 可重用控件
│   │   └── NetworkCaptureControl.xaml.cs
│   └── README.md                              # 项目说明
└── SimpleNetworkDataCapturer.Demo/            # 演示项目
    ├── MainWindow.xaml                        # 演示窗口
    ├── MainWindow.xaml.cs                     # 演示逻辑
    └── TestProtocols.cs                       # 协议测试
```

## 快速开始

### 1. 编译解决方案

```bash
dotnet build
```

### 2. 运行演示程序

```bash
dotnet run --project SimpleNetworkDataCapturer.Demo
```

**注意：** 由于需要访问网络接口，请以管理员权限运行应用程序。

### 3. 使用控件

如果您想在自己的 WPF 项目中使用这个控件：

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:capture="clr-namespace:SimpleNetworkDataCapturer.Controls;assembly=SimpleNetworkDataCapturer.Lib"
        Title="您的应用" Height="600" Width="800">
    <Grid>
        <capture:NetworkCaptureControl x:Name="NetworkCapture" />
    </Grid>
</Window>
```

## 控件 API

### 主要方法

- `StartCaptureAsync()` - 开始抓包
- `StopCapture()` - 停止抓包
- `ClearPackets()` - 清空数据包
- `GetPackets()` - 获取当前数据包列表
- `GetStatistics()` - 获取统计信息

### 过滤功能

- `FilterService` - 获取过滤服务实例
- **基础过滤**：支持按源地址、目标地址、端口、协议类型、内容进行过滤
- **高级过滤**：支持复杂的规则组合关系（AND/OR/N-of-M）
- 支持多种操作符：包含、等于、不等于、大于、小于、正则表达式
- 提供可视化的过滤规则管理界面
- 支持规则组的创建、编辑、删除和管理

### 事件

- `PacketCaptured` - 数据包到达事件
- `ErrorOccurred` - 错误事件

### 属性

- `IsCapturing` - 是否正在抓包

## 使用示例

```csharp
// 在代码中使用控件
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 订阅事件
        NetworkCapture.PacketCaptured += OnPacketCaptured;
        NetworkCapture.ErrorOccurred += OnErrorOccurred;
    }

    private void OnPacketCaptured(object sender, NetworkPacket packet)
    {
        // 处理捕获的数据包
        Console.WriteLine($"捕获到数据包: {packet.DisplayInfo}");

        // 根据协议类型进行不同处理
        switch (packet.Protocol.ToUpper())
        {
            case "HTTP":
                Console.WriteLine("检测到HTTP流量");
                break;
            case "HTTPS":
                Console.WriteLine("检测到HTTPS流量");
                break;
            case "DNS":
                Console.WriteLine("检测到DNS查询");
                break;
        }
    }

    private void OnErrorOccurred(object sender, string error)
    {
        // 处理错误
        MessageBox.Show($"抓包错误: {error}");
    }

    // 使用基础过滤功能
    private void SetupBasicFiltering()
    {
        var filterService = NetworkCapture.FilterService;

        // 添加过滤规则：只显示HTTP协议
        var httpRule = new FilterRule
        {
            Name = "HTTP流量",
            Description = "只显示HTTP协议的数据包",
            Type = FilterType.Protocol,
            Operator = FilterOperator.Equals,
            Value = "HTTP",
            IsEnabled = true
        };
        filterService.AddFilterRule(httpRule);

        // 添加过滤规则：过滤本地流量
        var localRule = new FilterRule
        {
            Name = "本地流量",
            Description = "只显示本地地址的流量",
            Type = FilterType.SourceAddress,
            Operator = FilterOperator.Contains,
            Value = "127.0.0.1",
            IsEnabled = true
        };
        filterService.AddFilterRule(localRule);

        // 启用过滤
        filterService.IsFilterEnabled = true;
    }

    // 使用高级过滤功能
    private void SetupAdvancedFiltering()
    {
        var filterService = NetworkCapture.FilterService;

        // 创建规则组：Web流量（HTTP或HTTPS）
        var webGroup = new FilterRuleGroup
        {
            Name = "Web流量",
            Description = "显示HTTP或HTTPS协议的数据包",
            Relation = FilterGroupRelation.Any,
            IsEnabled = true
        };

        // 添加HTTP规则到组
        webGroup.Rules.Add(new FilterRule
        {
            Name = "HTTP协议",
            Type = FilterType.Protocol,
            Operator = FilterOperator.Equals,
            Value = "HTTP",
            IsEnabled = true
        });

        // 添加HTTPS规则到组
        webGroup.Rules.Add(new FilterRule
        {
            Name = "HTTPS协议",
            Type = FilterType.Protocol,
            Operator = FilterOperator.Equals,
            Value = "HTTPS",
            IsEnabled = true
        });

        filterService.AddFilterRuleGroup(webGroup);

        // 创建规则组：重要端口流量（需要满足端口和协议两个条件）
        var importantPortsGroup = new FilterRuleGroup
        {
            Name = "重要端口流量",
            Description = "显示80、443、8080端口的HTTP/HTTPS流量",
            Relation = FilterGroupRelation.All,
            IsEnabled = true
        };

        // 添加端口规则
        importantPortsGroup.Rules.Add(new FilterRule
        {
            Name = "Web端口",
            Type = FilterType.DestinationPort,
            Operator = FilterOperator.Contains,
            Value = "80,443,8080",
            IsEnabled = true
        });

        // 添加协议规则
        importantPortsGroup.Rules.Add(new FilterRule
        {
            Name = "Web协议",
            Type = FilterType.Protocol,
            Operator = FilterOperator.Contains,
            Value = "HTTP,HTTPS",
            IsEnabled = true
        });

        filterService.AddFilterRuleGroup(importantPortsGroup);

        // 启用过滤
        filterService.IsFilterEnabled = true;
    }
}
```

## 数据包信息

每个捕获的数据包包含以下信息：

- **时间**: 精确到毫秒的捕获时间
- **源地址**: 数据包源 IP 地址
- **源端口**: 数据包源端口
- **目标地址**: 数据包目标 IP 地址
- **目标端口**: 数据包目标端口
- **协议**: 智能识别的协议类型
- **长度**: 数据包长度（字节）
- **可读数据**: 数据包内容的可读字符串表示
- **十六进制**: 数据包内容的十六进制表示

## 协议识别原理

### HTTP 协议识别

- 检测 HTTP 请求方法（GET、POST、PUT、DELETE 等）
- 检测 HTTP 响应状态行（HTTP/1.1 200 OK 等）

### HTTPS 协议识别

- 检测 TLS 握手消息类型（0x16、0x17、0x15）
- 支持 TLS 1.0、1.1、1.2、1.3 版本

### 其他协议识别

- 基于端口号识别（DNS:53、DHCP:67/68 等）
- 基于协议特征识别（FTP、SMTP、POP3、IMAP 等）

## 错误处理

- **数据包长度检查**: 过滤掉过短的数据包
- **异常捕获**: 单个数据包解析失败不影响其他数据包
- **调试日志**: 详细的错误信息记录
- **优雅降级**: 无法识别的协议显示为"TCP"或"UDP"

## 注意事项

1. **管理员权限**: 网络抓包需要管理员权限，请确保以管理员身份运行
2. **防火墙**: 某些防火墙可能会阻止抓包功能
3. **性能**: 大量数据包可能会影响性能，建议适当限制显示数量
4. **隐私**: 请遵守相关法律法规，不要用于非法用途
5. **协议识别**: HTTPS 流量由于加密，只能识别为 HTTPS，无法查看具体内容

## 更新日志

### v3.0.0 (2024-12-19)

- ✅ **项目架构优化**
  - 将 `SimpleNetworkDataCapturer.Lib` 从 WPF 项目改为库项目
  - 删除不必要的 WPF 项目文件（MainWindow.xaml、App.xaml 等）
  - 更清晰的架构分离，专注于提供可重用控件库
- ✅ **高级过滤规则组功能**
  - 支持复杂的规则组合关系：全部满足（AND）、任意满足（OR）、指定数量满足（N-of-M）
  - 提供规则组管理界面，支持创建、编辑、删除规则组
  - 每个规则组可以包含多个过滤规则，支持不同的组合逻辑
  - 规则组与单个规则并存，提供更灵活的过滤策略
- ✅ **过滤功能增强**
  - 分离基础过滤和高级过滤功能
  - 基础过滤：简单的单个规则管理
  - 高级过滤：复杂的规则组管理
  - 支持规则组的持久化存储和加载

### v2.0.0 (2024-12-19)

- ✅ 新增智能过滤功能
  - 支持按源地址、目标地址、端口、协议类型、内容过滤
  - 支持多种操作符：包含、等于、不等于、大于、小于、正则表达式
  - 提供可视化的过滤规则管理界面
- ✅ 优化协议统计显示
  - 详细显示 HTTP、HTTPS、DNS、DHCP、ICMP 等协议数量
  - 只有无法识别的协议才计入"其他"
  - 支持水平滚动显示更多统计信息
- ✅ 改进控件架构
  - 过滤功能集成到核心控件中
  - 提供 FilterService 属性供外部访问
  - 优化用户界面布局

### v1.0.0 (2024-12-18)

- ✅ 基础抓包功能
- ✅ 多协议支持（TCP、UDP、HTTP、HTTPS、DNS、DHCP 等）
- ✅ 工业风 UI 设计
- ✅ 可重用 WPF 控件

### v1.1.0

- ✅ 修复数据包解析时的数组越界错误
- ✅ 新增 HTTP/HTTPS 协议识别
- ✅ 新增 DNS、DHCP、FTP、SMTP、POP3、IMAP 协议识别
- ✅ 改进错误处理机制
- ✅ 优化协议统计逻辑

### v1.0.0

- ✅ 基础抓包功能
- ✅ TCP/UDP 协议支持
- ✅ WPF 控件封装

## 许可证

本项目仅供学习和研究使用，请遵守相关法律法规。

## 贡献

欢迎提交 Issue 和 Pull Request 来改进这个项目。
