using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Prism.Commands;
using Prism.Mvvm;

namespace SOReplaceLabel.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private static readonly Version? version = Assembly.GetExecutingAssembly().GetName().Version;
    private static readonly string? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
    private string _title = $"ラベル自動印刷 - {assemblyName} {version}";
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    public MainWindowViewModel()
    {

    }
}
