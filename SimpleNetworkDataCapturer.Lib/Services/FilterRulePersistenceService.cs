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
    public async Task SaveFilterRulesAsync(IEnumerable<FilterRule> rules)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(rules, options);
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
            
            var rules = JsonSerializer.Deserialize<List<FilterRule>>(json, options);
            return rules ?? CreateDefaultRules();
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
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigFilePath()
    {
        return _filterRulesFilePath;
    }
} 