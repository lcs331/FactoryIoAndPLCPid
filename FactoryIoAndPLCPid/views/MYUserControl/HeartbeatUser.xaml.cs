using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FactoryIoAndPLCPid.views.MYUserControl
{
    /// <summary>
    /// HeartbeatUser.xaml 的交互逻辑
    /// </summary>
    public partial class HeartbeatUser : UserControl
    {
        public static readonly DependencyProperty IsBeatingProperty =
    DependencyProperty.Register("IsBeating", typeof(bool), typeof(HeartbeatUser),
    new PropertyMetadata(false, OnIsBeatingChanged));

        public static readonly DependencyProperty TheBackgroundProperty =
            DependencyProperty.Register("TheBackground", typeof(Brush), typeof(HeartbeatUser),
            new PropertyMetadata(Brushes.Red, OnBackgroundChanged));

        public bool IsBeating
        {
            get => (bool)GetValue(IsBeatingProperty);
            set => SetValue(IsBeatingProperty, value);
        }

        public Brush TheBackground
        {
            get => (Brush)GetValue(TheBackgroundProperty);
            set => SetValue(TheBackgroundProperty, value);
        }

        // 回调方法：处理 IsBeating 变化
        private static void OnIsBeatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HeartbeatUser)d;
            bool isBeating = (bool)e.NewValue;

            if (isBeating)
                VisualStateManager.GoToState(control, "Beating", true);
            else
                VisualStateManager.GoToState(control, "Stopped", true);
        }

        // 回调方法：处理 TheBackground 变化
        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HeartbeatUser)d;
            Brush newBackground = (Brush)e.NewValue;
            // 更新背景颜色，或执行任何其他逻辑
            control.Background = newBackground;
        }
        public HeartbeatUser()
        {
            InitializeComponent();
        }
    }
}
