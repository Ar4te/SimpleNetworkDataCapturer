using System.Collections.ObjectModel;
using System.Windows;
using SimpleNetworkDataCapturer.Models;
using SimpleNetworkDataCapturer.Services;

namespace SimpleNetworkDataCapturer.Controls;

public partial class FilterDialog : Window
{
    private readonly PacketFilterService _filterService;
    private readonly ObservableCollection<FilterRule> _rules;
    private FilterRule? _currentRule;

    public FilterDialog(PacketFilterService filterService)
    {
        InitializeComponent();
        _filterService = filterService;
        _rules = new ObservableCollection<FilterRule>();
        
        LoadRules();
        RulesListBox.ItemsSource = _rules;
        
        // 设置默认值
        FilterTypeComboBox.SelectedIndex = 0;
        FilterOperatorComboBox.SelectedIndex = 0;
        RuleEnabledCheckBox.IsChecked = true;
    }

    private void LoadRules()
    {
        _rules.Clear();
        foreach (var rule in _filterService.FilterRules)
        {
            _rules.Add(rule);
        }
    }

    private void RulesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (RulesListBox.SelectedItem is FilterRule selectedRule)
        {
            LoadRuleToEditor(selectedRule);
        }
        else
        {
            ClearEditor();
        }
    }

    private void LoadRuleToEditor(FilterRule rule)
    {
        _currentRule = rule;
        
        RuleNameTextBox.Text = rule.Name;
        RuleDescriptionTextBox.Text = rule.Description;
        FilterValueTextBox.Text = rule.Value;
        RuleEnabledCheckBox.IsChecked = rule.IsEnabled;
        
        // 设置过滤类型
        FilterTypeComboBox.SelectedIndex = (int)rule.Type;
        
        // 设置操作符
        FilterOperatorComboBox.SelectedIndex = (int)rule.Operator;
        
        // 如果是协议类型，显示协议选择器
        if (rule.Type == FilterType.Protocol)
        {
            ProtocolSelectorPanel.Visibility = Visibility.Visible;
            ProtocolComboBox.SelectedItem = rule.Value;
        }
        else
        {
            ProtocolSelectorPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void ClearEditor()
    {
        _currentRule = null;
        
        RuleNameTextBox.Text = string.Empty;
        RuleDescriptionTextBox.Text = string.Empty;
        FilterValueTextBox.Text = string.Empty;
        RuleEnabledCheckBox.IsChecked = true;
        
        FilterTypeComboBox.SelectedIndex = 0;
        FilterOperatorComboBox.SelectedIndex = 0;
        ProtocolSelectorPanel.Visibility = Visibility.Collapsed;
    }

    private void FilterTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (FilterTypeComboBox.SelectedIndex == 4) // 协议类型
        {
            ProtocolSelectorPanel.Visibility = Visibility.Visible;
        }
        else
        {
            ProtocolSelectorPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void ProtocolComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ProtocolComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
        {
            FilterValueTextBox.Text = item.Content.ToString();
        }
    }

    private void AddRule_Click(object sender, RoutedEventArgs e)
    {
        var rule = CreateRuleFromEditor();
        if (rule != null)
        {
            _filterService.AddFilterRule(rule);
            _rules.Add(rule);
            ClearEditor();
        }
    }

    private void UpdateRule_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRule != null)
        {
            var updatedRule = CreateRuleFromEditor();
            if (updatedRule != null)
            {
                _filterService.RemoveFilterRule(_currentRule);
                _filterService.AddFilterRule(updatedRule);
                
                var index = _rules.IndexOf(_currentRule);
                if (index >= 0)
                {
                    _rules[index] = updatedRule;
                }
                
                ClearEditor();
            }
        }
    }

    private void DeleteRule_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRule != null)
        {
            if (MessageBox.Show("确定要删除这个规则吗？", "确认删除", 
                               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _filterService.RemoveFilterRule(_currentRule);
                _rules.Remove(_currentRule);
                ClearEditor();
            }
        }
    }

    private FilterRule? CreateRuleFromEditor()
    {
        if (string.IsNullOrWhiteSpace(RuleNameTextBox.Text))
        {
            MessageBox.Show("请输入规则名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (string.IsNullOrWhiteSpace(FilterValueTextBox.Text))
        {
            MessageBox.Show("请输入过滤值", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        return new FilterRule
        {
            Name = RuleNameTextBox.Text,
            Description = RuleDescriptionTextBox.Text,
            Type = (FilterType)FilterTypeComboBox.SelectedIndex,
            Operator = (FilterOperator)FilterOperatorComboBox.SelectedIndex,
            Value = FilterValueTextBox.Text,
            IsEnabled = RuleEnabledCheckBox.IsChecked ?? true
        };
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
} 