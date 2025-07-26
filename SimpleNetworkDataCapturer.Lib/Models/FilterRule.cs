namespace SimpleNetworkDataCapturer.Models;

/// <summary>
/// 过滤规则类型
/// </summary>
public enum FilterType
{
    /// <summary>
    /// 源地址
    /// </summary>
    SourceAddress,
    
    /// <summary>
    /// 源端口
    /// </summary>
    SourcePort,
    
    /// <summary>
    /// 目标地址
    /// </summary>
    DestinationAddress,
    
    /// <summary>
    /// 目标端口
    /// </summary>
    DestinationPort,
    
    /// <summary>
    /// 协议类型
    /// </summary>
    Protocol,
    
    /// <summary>
    /// 可读内容
    /// </summary>
    ReadableContent
}

/// <summary>
/// 过滤操作类型
/// </summary>
public enum FilterOperator
{
    /// <summary>
    /// 包含
    /// </summary>
    Contains,
    
    /// <summary>
    /// 等于
    /// </summary>
    Equals,
    
    /// <summary>
    /// 不等于
    /// </summary>
    NotEquals,
    
    /// <summary>
    /// 大于
    /// </summary>
    GreaterThan,
    
    /// <summary>
    /// 小于
    /// </summary>
    LessThan,
    
    /// <summary>
    /// 正则表达式
    /// </summary>
    Regex
}

/// <summary>
/// 过滤规则
/// </summary>
public class FilterRule
{
    /// <summary>
    /// 过滤类型
    /// </summary>
    public FilterType Type { get; set; }
    
    /// <summary>
    /// 操作符
    /// </summary>
    public FilterOperator Operator { get; set; }
    
    /// <summary>
    /// 过滤值
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 规则名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 规则描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;
} 