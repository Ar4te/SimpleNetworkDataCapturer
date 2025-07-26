using System.Collections.ObjectModel;
using System.Windows;
using SimpleNetworkDataCapturer.Lib.Models;
using SimpleNetworkDataCapturer.Lib.Services;

namespace SimpleNetworkDataCapturer.Controls;

/// <summary>
/// 高级过滤对话框
/// </summary>
public partial class AdvancedFilterDialog : Window
{
    private readonly PacketFilterService _filterService;
    private readonly ObservableCollection<FilterRuleGroup> _ruleGroups;
    private FilterRuleGroup? _currentGroup;
    private FilterRule? _currentRule;
    
    public AdvancedFilterDialog(PacketFilterService filterService)
    {
        InitializeComponent();
        _filterService = filterService;
        _ruleGroups = new ObservableCollection<FilterRuleGroup>();
        
        InitializeControls();
        LoadRuleGroups();
    }
    
    /// <summary>
    /// 初始化控件
    /// </summary>
    private void InitializeControls()
    {
        // 初始化关系类型组合框
        RelationComboBox.ItemsSource = Enum.GetValues<FilterGroupRelation>();
        RelationComboBox.SelectedIndex = 0;
        
        // 绑定规则组列表
        RuleGroupsListBox.ItemsSource = _ruleGroups;
        
        // 设置列表框显示模板
        RuleGroupsListBox.DisplayMemberPath = "Name";
        GroupRulesListBox.DisplayMemberPath = "Name";
    }
    
    /// <summary>
    /// 加载规则组
    /// </summary>
    private void LoadRuleGroups()
    {
        _ruleGroups.Clear();
        foreach (var group in _filterService.FilterRuleGroups)
        {
            _ruleGroups.Add(group);
        }
    }
    
    /// <summary>
    /// 规则组列表选择改变事件
    /// </summary>
    private void RuleGroupsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _currentGroup = RuleGroupsListBox.SelectedItem as FilterRuleGroup;
        LoadGroupToEditor();
    }
    
    /// <summary>
    /// 加载规则组到编辑器
    /// </summary>
    private void LoadGroupToEditor()
    {
        if (_currentGroup == null)
        {
            ClearEditor();
            return;
        }
        
        GroupNameTextBox.Text = _currentGroup.Name;
        GroupDescriptionTextBox.Text = _currentGroup.Description;
        RelationComboBox.SelectedItem = _currentGroup.Relation;
        RequiredCountTextBox.Text = _currentGroup.RequiredCount.ToString();
        GroupEnabledCheckBox.IsChecked = _currentGroup.IsEnabled;
        
        // 加载组内规则
        GroupRulesListBox.ItemsSource = _currentGroup.Rules;
    }
    
    /// <summary>
    /// 清空编辑器
    /// </summary>
    private void ClearEditor()
    {
        GroupNameTextBox.Text = string.Empty;
        GroupDescriptionTextBox.Text = string.Empty;
        RelationComboBox.SelectedIndex = 0;
        RequiredCountTextBox.Text = "1";
        GroupEnabledCheckBox.IsChecked = true;
        GroupRulesListBox.ItemsSource = null;
        _currentGroup = null;
    }
    
    /// <summary>
    /// 关系类型选择改变事件
    /// </summary>
    private void RelationComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var relation = (FilterGroupRelation)RelationComboBox.SelectedItem;
        RequiredCountLabel.Visibility = relation == FilterGroupRelation.Count ? Visibility.Visible : Visibility.Collapsed;
        RequiredCountTextBox.Visibility = relation == FilterGroupRelation.Count ? Visibility.Visible : Visibility.Collapsed;
    }
    
    /// <summary>
    /// 组内规则列表选择改变事件
    /// </summary>
    private void GroupRulesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _currentRule = GroupRulesListBox.SelectedItem as FilterRule;
    }
    
    /// <summary>
    /// 添加规则组按钮点击事件
    /// </summary>
    private void AddRuleGroup_Click(object sender, RoutedEventArgs e)
    {
        var newGroup = new FilterRuleGroup
        {
            Name = "新规则组",
            Description = "新创建的规则组",
            Relation = FilterGroupRelation.All,
            RequiredCount = 1,
            IsEnabled = true
        };
        
        _ruleGroups.Add(newGroup);
        RuleGroupsListBox.SelectedItem = newGroup;
    }
    
    /// <summary>
    /// 删除规则组按钮点击事件
    /// </summary>
    private void DeleteRuleGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_currentGroup != null)
        {
            _ruleGroups.Remove(_currentGroup);
            ClearEditor();
        }
    }
    
    /// <summary>
    /// 添加规则到组按钮点击事件
    /// </summary>
    private void AddRuleToGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_currentGroup == null)
        {
            MessageBox.Show("请先选择一个规则组", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        
        var newRule = new FilterRule
        {
            Name = "新规则",
            Description = "新创建的规则",
            Type = FilterType.Protocol,
            Operator = FilterOperator.Equals,
            Value = string.Empty,
            IsEnabled = true
        };
        
        _currentGroup.Rules.Add(newRule);
        GroupRulesListBox.Items.Refresh();
    }
    
    /// <summary>
    /// 从组删除规则按钮点击事件
    /// </summary>
    private void DeleteRuleFromGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_currentGroup != null && _currentRule != null)
        {
            _currentGroup.Rules.Remove(_currentRule);
            GroupRulesListBox.Items.Refresh();
        }
    }
    
    /// <summary>
    /// 确定按钮点击事件
    /// </summary>
    private void OK_Click(object sender, RoutedEventArgs e)
    {
        // 保存当前编辑的规则组
        if (_currentGroup != null)
        {
            SaveCurrentGroup();
        }
        
        // 更新过滤服务
        _filterService.ClearFilterRuleGroups();
        foreach (var group in _ruleGroups)
        {
            _filterService.AddFilterRuleGroup(group);
        }
        
        DialogResult = true;
        Close();
    }
    
    /// <summary>
    /// 保存当前规则组
    /// </summary>
    private void SaveCurrentGroup()
    {
        if (_currentGroup == null) return;
        
        _currentGroup.Name = GroupNameTextBox.Text;
        _currentGroup.Description = GroupDescriptionTextBox.Text;
        _currentGroup.Relation = (FilterGroupRelation)RelationComboBox.SelectedItem;
        
        if (int.TryParse(RequiredCountTextBox.Text, out var count))
        {
            _currentGroup.RequiredCount = count;
        }
        
        _currentGroup.IsEnabled = GroupEnabledCheckBox.IsChecked ?? true;
    }
    
    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
} 