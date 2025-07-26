using SimpleNetworkDataCapturer.Lib.Models;
using System.IO;
using System.Text.Json;

namespace SimpleNetworkDataCapturer.Lib.Services;

/// <summary>
/// 过滤规则持久化服务
/// </summary>
public class FilterRulePersistenceService
{
    private readonly string _filterRulesFilePath;
    
    public FilterRulePersistenceService()
    {
        var appDataPath = AppDomain.CurrentDomain.BaseDirectory;

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        
        _filterRulesFilePath = Path.Combine(appDataPath, "filter_rules.json");
    }
    
    /// <summary>
    /// 保存过滤规则到文件
    /// </summary>
    public async Task SaveFilterRulesAsync(IEnumerable<FilterRule> rules, IEnumerable<FilterRuleGroup> ruleGroups = null)
    {
        try
        {
            var data = new
            {
                Rules = rules,
                RuleGroups = ruleGroups ?? new List<FilterRuleGroup>()
            };
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(_filterRulesFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存过滤规则失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 从文件加载过滤规则
    /// </summary>
    public async Task<List<FilterRule>> LoadFilterRulesAsync()
    {
        try
        {
            if (!File.Exists(_filterRulesFilePath))
            {
                return CreateDefaultRules();
            }
            
            var json = await File.ReadAllTextAsync(_filterRulesFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // 尝试加载新格式（包含规则组）
            try
            {
                var data = JsonSerializer.Deserialize<dynamic>(json, options);
                if (data != null && data.GetProperty("rules").ValueKind == JsonValueKind.Array)
                {
                    var newFormatRules = JsonSerializer.Deserialize<List<FilterRule>>(data.GetProperty("rules").GetRawText(), options);
                    return newFormatRules ?? CreateDefaultRules();
                }
            }
            catch
            {
                // 如果新格式解析失败，尝试旧格式
            }
            
            // 尝试旧格式（只有规则列表）
            var oldFormatRules = JsonSerializer.Deserialize<List<FilterRule>>(json, options);
            return oldFormatRules ?? CreateDefaultRules();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载过滤规则失败: {ex.Message}");
            return CreateDefaultRules();
        }
    }
    
    /// <summary>
    /// 创建默认过滤规则
    /// </summary>
    private List<FilterRule> CreateDefaultRules()
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
    
    /// <summary>
    /// 清除过滤规则文件
    /// </summary>
    public void ClearFilterRulesFile()
    {
        try
        {
            if (File.Exists(_filterRulesFilePath))
            {
                File.Delete(_filterRulesFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"删除过滤规则文件失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 从文件加载过滤规则组
    /// </summary>
    public async Task<List<FilterRuleGroup>> LoadFilterRuleGroupsAsync()
    {
        try
        {
            if (!File.Exists(_filterRulesFilePath))
            {
                return new List<FilterRuleGroup>();
            }
            
            var json = await File.ReadAllTextAsync(_filterRulesFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // 尝试加载新格式（包含规则组）
            try
            {
                var data = JsonSerializer.Deserialize<dynamic>(json, options);
                if (data != null && data.GetProperty("ruleGroups").ValueKind == JsonValueKind.Array)
                {
                    var ruleGroups = JsonSerializer.Deserialize<List<FilterRuleGroup>>(data.GetProperty("ruleGroups").GetRawText(), options);
                    return ruleGroups ?? new List<FilterRuleGroup>();
                }
            }
            catch
            {
                // 如果新格式解析失败，返回空列表
            }
            
            return new List<FilterRuleGroup>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载过滤规则组失败: {ex.Message}");
            return new List<FilterRuleGroup>();
        }
    }
    
    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigFilePath()
    {
        return _filterRulesFilePath;
    }
} 