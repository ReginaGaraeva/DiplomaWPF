﻿#pragma checksum "..\..\Statistics.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "46C4BA760D2D3AB211A28D7B50A28B65"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace FIeldDataAnalyzer {
    
    
    /// <summary>
    /// Statistics
    /// </summary>
    public partial class Statistics : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Forms.DataVisualization.Charting.Chart wellChart;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox T1CheckBox;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox T2CheckBox;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox P1CheckBox;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox P2CheckBox;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox GCheckBox;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\Statistics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox wellsComboBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/FIeldDataAnalyzer;component/statistics.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Statistics.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 7 "..\..\Statistics.xaml"
            ((FIeldDataAnalyzer.Statistics)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded_1);
            
            #line default
            #line hidden
            return;
            case 2:
            this.wellChart = ((System.Windows.Forms.DataVisualization.Charting.Chart)(target));
            return;
            case 3:
            this.T1CheckBox = ((System.Windows.Controls.CheckBox)(target));
            
            #line 29 "..\..\Statistics.xaml"
            this.T1CheckBox.Checked += new System.Windows.RoutedEventHandler(this.T1CheckBox_Checked);
            
            #line default
            #line hidden
            
            #line 29 "..\..\Statistics.xaml"
            this.T1CheckBox.Unchecked += new System.Windows.RoutedEventHandler(this.T1CheckBox_Unchecked);
            
            #line default
            #line hidden
            return;
            case 4:
            this.T2CheckBox = ((System.Windows.Controls.CheckBox)(target));
            
            #line 30 "..\..\Statistics.xaml"
            this.T2CheckBox.Unchecked += new System.Windows.RoutedEventHandler(this.T2CheckBox_Unchecked);
            
            #line default
            #line hidden
            
            #line 30 "..\..\Statistics.xaml"
            this.T2CheckBox.Checked += new System.Windows.RoutedEventHandler(this.T2CheckBox_Checked);
            
            #line default
            #line hidden
            return;
            case 5:
            this.P1CheckBox = ((System.Windows.Controls.CheckBox)(target));
            
            #line 31 "..\..\Statistics.xaml"
            this.P1CheckBox.Unchecked += new System.Windows.RoutedEventHandler(this.P1CheckBox_Unchecked);
            
            #line default
            #line hidden
            
            #line 31 "..\..\Statistics.xaml"
            this.P1CheckBox.Checked += new System.Windows.RoutedEventHandler(this.P1CheckBox_Checked);
            
            #line default
            #line hidden
            return;
            case 6:
            this.P2CheckBox = ((System.Windows.Controls.CheckBox)(target));
            
            #line 32 "..\..\Statistics.xaml"
            this.P2CheckBox.Unchecked += new System.Windows.RoutedEventHandler(this.P2CheckBox_Unchecked);
            
            #line default
            #line hidden
            
            #line 32 "..\..\Statistics.xaml"
            this.P2CheckBox.Checked += new System.Windows.RoutedEventHandler(this.P2CheckBox_Checked);
            
            #line default
            #line hidden
            return;
            case 7:
            this.GCheckBox = ((System.Windows.Controls.CheckBox)(target));
            
            #line 33 "..\..\Statistics.xaml"
            this.GCheckBox.Unchecked += new System.Windows.RoutedEventHandler(this.GCheckBox_Unchecked);
            
            #line default
            #line hidden
            
            #line 33 "..\..\Statistics.xaml"
            this.GCheckBox.Checked += new System.Windows.RoutedEventHandler(this.GCheckBox_Checked);
            
            #line default
            #line hidden
            return;
            case 8:
            this.wellsComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 41 "..\..\Statistics.xaml"
            this.wellsComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.wellsComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
