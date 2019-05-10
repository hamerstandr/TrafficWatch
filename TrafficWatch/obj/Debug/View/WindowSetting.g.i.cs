﻿#pragma checksum "..\..\..\View\WindowSetting.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "EEDA6EF7E2DA4E7A7868D828AA079B993ADCC012E1B2CFE6370B41F7B1505682"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
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
using TrafficWatch;


namespace TrafficWatch {
    
    
    /// <summary>
    /// WindowSetting
    /// </summary>
    public partial class WindowSetting : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 295 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid GBlur;
        
        #line default
        #line hidden
        
        
        #line 301 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonMinimized;
        
        #line default
        #line hidden
        
        
        #line 317 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonMaximized;
        
        #line default
        #line hidden
        
        
        #line 318 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonClose;
        
        #line default
        #line hidden
        
        
        #line 335 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbInterface;
        
        #line default
        #line hidden
        
        
        #line 344 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblBytesSent;
        
        #line default
        #line hidden
        
        
        #line 346 "..\..\..\View\WindowSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblBytesReceived;
        
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
            System.Uri resourceLocater = new System.Uri("/TrafficWatch;component/view/windowsetting.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\WindowSetting.xaml"
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
            this.GBlur = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.ButtonMinimized = ((System.Windows.Controls.Button)(target));
            
            #line 301 "..\..\..\View\WindowSetting.xaml"
            this.ButtonMinimized.Click += new System.Windows.RoutedEventHandler(this.ButtonMinimized_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ButtonMaximized = ((System.Windows.Controls.Button)(target));
            
            #line 317 "..\..\..\View\WindowSetting.xaml"
            this.ButtonMaximized.Click += new System.Windows.RoutedEventHandler(this.ButtonMaximized_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ButtonClose = ((System.Windows.Controls.Button)(target));
            
            #line 318 "..\..\..\View\WindowSetting.xaml"
            this.ButtonClose.Click += new System.Windows.RoutedEventHandler(this.ButtonClose_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.CmbInterface = ((System.Windows.Controls.ComboBox)(target));
            
            #line 335 "..\..\..\View\WindowSetting.xaml"
            this.CmbInterface.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbInterface_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.lblBytesSent = ((System.Windows.Controls.Label)(target));
            return;
            case 7:
            this.lblBytesReceived = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

