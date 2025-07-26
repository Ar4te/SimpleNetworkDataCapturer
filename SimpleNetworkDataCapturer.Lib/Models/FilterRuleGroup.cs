namespace SimpleNetworkDataCapturer.Lib.Models;

/// <summary>
/// 过滤规则组关系类型
/// </summary>
public enum FilterGroupRelation
{
    /// <summary>
    /// 所有规则都必须满足（AND）
    /// </summary>
    All,
    
    /// <summary>
    /// 任意一个规则满足即可（OR）
    /// </summary>
    Any,
    
    /// <summary>
    /// 指定数量的规则满足即可（N-of-M）
    /// </summary>
    Count
}

/// <summary>
/// 过滤规则组
/// </summary>
public class FilterRuleGroup
{
    /// <summary>
    /// 组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 组描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 组内规则列表
    /// </summary>
    public List<FilterRule> Rules { get; set; } = new();
    
    /// <summary>
    /// 规则关系类型
    /// </summary>
    public FilterGroupRelation Relation { get; set; } = FilterGroupRelation.All;
    
    /// <summary>
    /// 当关系类型为Count时，需要满足的规则数量
    /// </summary>
    public int RequiredCount { get; set; } = 1;
    
    /// <summary>
    /// 是否启用此规则组
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 检查数据包是否通过此规则组
    /// </summary>
    public bool IsPacketPassed(NetworkPacket packet)
    {
        if (!IsEnabled || Rules.Count == 0)
        {
            return true;
        }
        
        var passedRules = Rules.Where(r => r.IsEnabled && IsRuleMatched(packet, r)).ToList();
        
        return Relation switch
        {
            FilterGroupRelation.All => passedRules.Count == Rules.Count(r => r.IsEnabled),
            FilterGroupRelation.Any => passedRules.Count > 0,
            FilterGroupRelation.Count => passedRules.Count >= RequiredCount,
            _ => false
        };
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
            FilterOperator.Regex => System.Text.RegularExpressions.Regex.IsMatch(valueToCheck, rule.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase),
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
    /// 获取关系描述
    /// </summary>
    public string GetRelationDescription()
    {
        return Relation switch
        {
            FilterGroupRelation.All => "全部满足",
            FilterGroupRelation.Any => "任意满足",
            FilterGroupRelation.Count => $"满足{RequiredCount}个",
            _ => "未知"
        };
    }
} 