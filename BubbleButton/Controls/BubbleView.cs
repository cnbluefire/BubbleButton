using BubbleButton.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace BubbleButton.Controls
{
    public class BubbleView : Control
    {
        public BubbleView()
        {
            this.DefaultStyleKey = typeof(BubbleView);
            ForegroundPropertyChangedToken = RegisterPropertyChangedCallback(ForegroundProperty, ForegroundPropertyChanged);
        }

        Rectangle BubbleHost;
        Color ForegroundColor;

        Compositor _Compositor;
        Visual _HostVisual;
        ContainerVisual _BubblesVisual;

        List<Bubble> Bubbles;

        long ForegroundPropertyChangedToken;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BubbleHost = GetTemplateChild("BubbleHost") as Rectangle;
            BubbleHost.SizeChanged += BubbleHost_SizeChanged;
            SetupComposition();
        }

        private void SetupComposition()
        {
            _HostVisual = ElementCompositionPreview.GetElementVisual(BubbleHost);
            _Compositor = _HostVisual.Compositor;

            _BubblesVisual = _Compositor.CreateContainerVisual();
            _BubblesVisual.BindSize(_HostVisual);

            ElementCompositionPreview.SetElementChildVisual(BubbleHost, _BubblesVisual);
        }

        private void ClearBubbles()
        {
            Bubbles?.Clear();
            Bubbles = null;

            if (_BubblesVisual != null)
            {
                _BubblesVisual.Children.RemoveAll();
            }
        }

        private void CreateBubbles()
        {
            ClearBubbles();
            if (DesignMode.DesignModeEnabled) return;
            if (ForegroundColor != Colors.Transparent && this.ActualWidth > 0 && this.ActualHeight > 0)
            {
                var count = 20;

                var _Bubbles = new List<Bubble>();

                var duration = TimeSpan.FromSeconds(1d);

                for (int i = 0; i < count; i++)
                {
                    var bubble = new Bubble(_Compositor, new Size(this.ActualWidth, this.ActualHeight), ForegroundColor, duration, true);
                    bubble.AddTo(_BubblesVisual);
                    _Bubbles.Add(bubble);
                }
                for (int i = 0; i < count; i++)
                {
                    var bubble = new Bubble(_Compositor, new Size(this.ActualWidth, this.ActualHeight), ForegroundColor, duration, false);
                    bubble.AddTo(_BubblesVisual);
                    _Bubbles.Add(bubble);
                }
                Bubbles = _Bubbles;
            }
        }

        public void ShowBubbles()
        {
            if (Bubbles != null)
            {
                foreach (var bubble in Bubbles)
                {
                    bubble.Start();
                }
            }
        }

        private void BubbleHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateBubbles();
        }

        private void ForegroundPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (Foreground is SolidColorBrush brush)
            {
                ForegroundColor = brush.Color;
            }
            CreateBubbles();
        }



        public bool IsBubbing
        {
            get { return (bool)GetValue(IsBubbingProperty); }
            set { SetValue(IsBubbingProperty, value); }
        }

        public static readonly DependencyProperty IsBubbingProperty =
            DependencyProperty.Register("IsBubbing", typeof(bool), typeof(BubbleView), new PropertyMetadata(false, (s, a) =>
            {
                if(a.NewValue != a.OldValue)
                {
                    if(s is BubbleView sender)
                    {
                        if(a.NewValue is true)
                        {
                            sender.ShowBubbles();
                            sender.SetValue(IsBubbingProperty, false);
                        }
                    }
                }
            }));


    }
}
