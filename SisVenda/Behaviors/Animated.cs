using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Sisvenda.Behaviors
{
    // Attached properties that provide lightweight animations (press, focus, appear)
    // Centralizado para ser aplicado via Styles em XAML.
    public static class Animated
    {
        // Button press animations
        public static readonly BindableProperty IsPressScaleEnabledProperty =
            BindableProperty.CreateAttached(
                "IsPressScaleEnabled",
                typeof(bool),
                typeof(Animated),
                false,
                propertyChanged: OnIsPressScaleEnabledChanged);

        public static bool GetIsPressScaleEnabled(BindableObject view) => (bool)view.GetValue(IsPressScaleEnabledProperty);
        public static void SetIsPressScaleEnabled(BindableObject view, bool value) => view.SetValue(IsPressScaleEnabledProperty, value);

        // Entry focus animations
        public static readonly BindableProperty IsFocusScaleEnabledProperty =
            BindableProperty.CreateAttached(
                "IsFocusScaleEnabled",
                typeof(bool),
                typeof(Animated),
                false,
                propertyChanged: OnIsFocusScaleEnabledChanged);

        public static bool GetIsFocusScaleEnabled(BindableObject view) => (bool)view.GetValue(IsFocusScaleEnabledProperty);
        public static void SetIsFocusScaleEnabled(BindableObject view, bool value) => view.SetValue(IsFocusScaleEnabledProperty, value);

        // Frame/Card entrance animation (fade + translate)
        public static readonly BindableProperty IsCardEntranceEnabledProperty =
            BindableProperty.CreateAttached(
                "IsCardEntranceEnabled",
                typeof(bool),
                typeof(Animated),
                false,
                propertyChanged: OnIsCardEntranceEnabledChanged);

        public static bool GetIsCardEntranceEnabled(BindableObject view) => (bool)view.GetValue(IsCardEntranceEnabledProperty);
        public static void SetIsCardEntranceEnabled(BindableObject view, bool value) => view.SetValue(IsCardEntranceEnabledProperty, value);

        // Page fade on appearing
        public static readonly BindableProperty IsFadeOnAppearingProperty =
            BindableProperty.CreateAttached(
                "IsFadeOnAppearing",
                typeof(bool),
                typeof(Animated),
                false,
                propertyChanged: OnIsFadeOnAppearingChanged);

        public static bool GetIsFadeOnAppearing(BindableObject view) => (bool)view.GetValue(IsFadeOnAppearingProperty);
        public static void SetIsFadeOnAppearing(BindableObject view, bool value) => view.SetValue(IsFadeOnAppearingProperty, value);

        // Keep track of handlers to allow detach
        private static readonly Dictionary<VisualElement, (EventHandler Pressed, EventHandler Released)> _buttonHandlers = new();
        private static readonly Dictionary<Entry, (EventHandler<FocusEventArgs> Focused, EventHandler<FocusEventArgs> Unfocused)> _entryHandlers = new();
        private static readonly HashSet<VisualElement> _cardAnimated = new();

        private static void OnIsPressScaleEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Button btn)
            {
                var enabled = (bool)newValue;
                if (enabled)
                {
                    EventHandler pressed = async (s, e) => await OnButtonPressed(btn);
                    EventHandler released = async (s, e) => await OnButtonReleased(btn);
                    btn.Pressed += pressed;
                    btn.Released += released;
                    _buttonHandlers[btn] = (pressed, released);
                }
                else
                {
                    if (_buttonHandlers.TryGetValue(btn, out var handlers))
                    {
                        btn.Pressed -= handlers.Pressed;
                        btn.Released -= handlers.Released;
                        _buttonHandlers.Remove(btn);
                    }
                }
            }
        }

        private static async Task OnButtonPressed(Button btn)
        {
            if (!AreAnimationsAllowed())
                return;
            try
            {
                // quick scale + subtle fade for press feedback
                await Task.WhenAll(
                    btn.ScaleTo(0.96, 120, Easing.CubicInOut),
                    btn.FadeTo(0.95, 120, Easing.CubicInOut)
                );
            }
            catch { }
        }

        private static async Task OnButtonReleased(Button btn)
        {
            if (!AreAnimationsAllowed())
                return;
            try
            {
                await Task.WhenAll(
                    btn.ScaleTo(1.0, 140, Easing.CubicInOut),
                    btn.FadeTo(1.0, 140, Easing.CubicInOut)
                );
            }
            catch { }
        }

        private static void OnIsFocusScaleEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Entry entry)
            {
                var enabled = (bool)newValue;
                if (enabled)
                {
                    EventHandler<FocusEventArgs> focused = async (s, e) => { if (AreAnimationsAllowed()) await entry.ScaleTo(1.02, 150, Easing.CubicInOut); };
                    EventHandler<FocusEventArgs> unfocused = async (s, e) => { if (AreAnimationsAllowed()) await entry.ScaleTo(1.0, 150, Easing.CubicInOut); };
                    entry.Focused += focused;
                    entry.Unfocused += unfocused;
                    _entryHandlers[entry] = (focused, unfocused);
                }
                else
                {
                    if (_entryHandlers.TryGetValue(entry, out var handlers))
                    {
                        entry.Focused -= handlers.Focused;
                        entry.Unfocused -= handlers.Unfocused;
                        _entryHandlers.Remove(entry);
                    }
                }
            }
        }

        private static void OnIsCardEntranceEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is VisualElement element)
            {
                var enabled = (bool)newValue;
                if (enabled)
                {
                    void handler(object s, EventArgs e)
                    {
                        // run once
                        if (_cardAnimated.Contains(element))
                            return;
                        _cardAnimated.Add(element);
                        if (AreAnimationsAllowed())
                            _ = RunCardEntranceAsync(element);
                    }

                    // try SizeChanged as a lightweight 'loaded' indicator
                    element.SizeChanged += (s, e) => handler(s, e);
                    // also run when handler already exists
                    if (element.Handler != null)
                    {
                        handler(element, EventArgs.Empty);
                    }
                }
            }
        }

        private static async Task RunCardEntranceAsync(VisualElement element)
        {
            if (!AreAnimationsAllowed())
                return;
            try
            {
                element.Opacity = 0;
                element.TranslationY = 8;
                await Task.Delay(10);
                await Task.WhenAll(
                    element.FadeTo(1, 260, Easing.SinInOut),
                    element.TranslateTo(0, 0, 260, Easing.SpringOut)
                );
            }
            catch { }
        }

        private static void OnIsFadeOnAppearingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Page page)
            {
                var enabled = (bool)newValue;
                if (enabled)
                {
                    page.Appearing += OnPageAppearing;
                }
                else
                {
                    page.Appearing -= OnPageAppearing;
                }
            }
        }

        private static async void OnPageAppearing(object sender, EventArgs e)
        {
            if (sender is Page page)
            {
                if (!AreAnimationsAllowed())
                    return;
                try
                {
                    // initial subtle fade/translate for page navigation
                    page.Opacity = 0;
                    page.TranslationY = 6;
                    await Task.WhenAll(
                        page.FadeTo(1, 240, Easing.CubicInOut),
                        page.TranslateTo(0, 0, 240, Easing.CubicInOut)
                    );
                }
                catch { }
            }
        }

        // Global check: reads App resource AnimationsEnabled. Future extension: respect OS Reduce Motion
        private static bool AreAnimationsAllowed()
        {
            try
            {
                if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey("AnimationsEnabled"))
                {
                    if (Application.Current.Resources["AnimationsEnabled"] is bool b)
                        return b;
                }
            }
            catch { }
            return true;
        }
    }
}
