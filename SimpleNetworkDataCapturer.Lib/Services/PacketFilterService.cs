using System.Text.RegularExpressions;
using SimpleNetworkDataCapturer.Lib.Models;

namespace SimpleNetworkDataCapturer.Lib.Services;

/// <summary>
/// 数据包过滤服务
/// </summary>
public class PacketFilterService
{
    private readonly List<FilterRule> _filterRules = new();
    private readonly FilterRulePersistenceService _persistenceService;
    
    /// <summary>
    /// 过滤规则列表
    /// </summary>
    public IReadOnlyList<FilterRule> FilterRules => _filterRules.AsReadOnly();
    
    /// <summary>
    /// 过滤规则变化事件
    /// </summary>
    public event EventHandler? FilterRulesChanged;
    
    /// <summary>
    /// 是否启用过滤
    /// </summary>
    public bool IsFilterEnabled { get; set; } = false;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public PacketFilterService()
    {
        _persistenceService = new FilterRulePersistenceService();
    }
    
    /// <summary>
    /// 添加过滤规则
    /// </summary>
    public void AddFilterRule(FilterRule rule)
    {
        _filterRules.Add(rule);
        SaveFilterRulesAsync();
        FilterRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 移除过滤规则
    /// </summary>
    public void RemoveFilterRule(FilterRule rule)
    {
        _filterRules.Remove(rule);
        SaveFilterRulesAsync();
        FilterRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 移除过滤规则
    /// </summary>
    public void RemoveFilterRule(int index)
    {
        if (index >= 0 && index < _filterRules.Count)
        {
            _filterRules.RemoveAt(index);
            SaveFilterRulesAsync();
            FilterRulesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// 清空所有过滤规则
    /// </summary>
    public void ClearFilterRules()
    {
        _filterRules.Clear();
        SaveFilterRulesAsync();
        FilterRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 检查数据包是否通过过滤
    /// </summary>
    public bool IsPacketPassed(NetworkPacket packet)
    {
        if (!IsFilterEnabled || _filterRules.Count == 0)
        {
            return true; // 不过滤
        }
        
        foreach (var rule in _filterRules.Where(r => r.IsEnabled))
        {
            if (!IsRuleMatched(packet, rule))
            {
                return false; // 不匹配任何规则，过滤掉
            }
        }
        
        return true; // 通过所有规则
    }
    
    /// <summary>
    /// 检查数据包是否匹配规则
    /// </summary>
    private bool IsRuleMatched(NetworkPacket packet, FilterRule rule)
    {
        string? valueToCheck = rule.Type switch
        {
            FilterType.SourceAddress => packet.SourceAddress,
            FilterType.SourcePort => packet.SourcePort.ToString(),
            FilterType.DestinationAddress => packet.DestinationAddress,
            FilterType.DestinationPort => packet.DestinationPort.ToString(),
            FilterType.Protocol => packet.Protocol,
            FilterType.ReadableContent => packet.ReadableData,
            _ => null
        };
        
        if (valueToCheck == null)
        {
            return false;
        }
        
        return rule.Operator switch
        {
            FilterOperator.Contains => valueToCheck.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Equals => string.Equals(valueToCheck, rule.Value, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotEquals => !string.Equals(valueToCheck, rule.Value, StringComparison.OrdinalIgnoreCase),
            FilterOperator.GreaterThan => TryCompareNumbers(valueToCheck, rule.Value, out var result) && result > 0,
            FilterOperator.LessThan => TryCompareNumbers(valueToCheck, rule.Value, out var result) && result < 0,
            FilterOperator.Regex => Regex.IsMatch(valueToCheck, rule.Value, RegexOptions.IgnoreCase),
            _ => false
        };
    }
    
    /// <summary>
    /// 尝试比较数字
    /// </summary>
    private bool TryCompareNumbers(string value1, string value2, out int result)
    {
        result = 0;
        
        if (!int.TryParse(value1, out var num1) || !int.TryParse(value2, out var num2))
        {
            return false;
        }
        
        result = num1.CompareTo(num2);
        return true;
    }
    
    /// <summary>
    /// 加载过滤规则
    /// </summary>
    public async Task LoadFilterRulesAsync()
    {
        var rules = await _persistenceService.LoadFilterRulesAsync();
        _filterRules.Clear();
        _filterRules.AddRange(rules);
        FilterRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 保存过滤规则
    /// </summary>
    private async void SaveFilterRulesAsync()
    {
        await _persistenceService.SaveFilterRulesAsync(_filterRules);
    }
    
    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigFilePath()
    {
        return _persistenceService.GetConfigFilePath();
    }
    
    /// <summary>
    /// 创建预定义过滤规则
    /// </summary>
    public static List<FilterRule> CreateDefaultRules()
    {
        return new List<FilterRule>
        {
            new FilterRule
            {
                Name = "HTTP流量",
                Description = "只显示HTTP协议的数据包",
                Type = FilterType.Protocol,
                Operator = FilterOperator.Equals,
                Value = "HTTP",
                IsEnabled = false
            },
            new FilterRule
            {
                Name = "HTTPS流量",
                Description = "只显示HTTPS协议的数据包",
                Type = FilterType.Protocol,
                Operator = FilterOperator.Equals,
                Value = "HTTPS",
                IsEnabled = false
            },
            new FilterRule
            {
                Name = "DNS查询",
                Description = "只显示DNS协议的数据包",
                Type = FilterType.Protocol,
                Operator = FilterOperator.Equals,
                Value = "DNS",
                IsEnabled = false
            },
            new FilterRule
            {
                Name = "本地流量",
                Description = "只显示本地地址的流量",
                Type = FilterType.SourceAddress,
                Operator = FilterOperator.Contains,
                Value = "127.0.0.1",
                IsEnabled = false
            },
            new FilterRule
            {
                Name = "Web流量",
                Description = "只显示80和443端口的流量",
                Type = FilterType.DestinationPort,
                Operator = FilterOperator.Equals,
                Value = "80",
                IsEnabled = false
            }
        };
    }
} 