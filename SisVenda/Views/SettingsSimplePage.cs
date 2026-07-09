using Microsoft.Maui.Controls;

namespace Sisvenda.Views
{
    // Simple programmatic settings page to toggle global animations
    public class SettingsSimplePage : ContentPage
    {
        Switch _animationsSwitch;

        public SettingsSimplePage()
        {
            Title = "Configurações";

            _animationsSwitch = new Switch();
            _animationsSwitch.Toggled += OnAnimationsToggled;

            // Initialize value
            try
            {
                if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey("AnimationsEnabled"))
                {
                    if (Application.Current.Resources["AnimationsEnabled"] is bool b)
                        _animationsSwitch.IsToggled = b;
                    else
                        _animationsSwitch.IsToggled = true;
                }
                else
                {
                    _animationsSwitch.IsToggled = true;
                }
            }
            catch
            {
                _animationsSwitch.IsToggled = true;
            }

            var layout = new StackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    new Label { Text = "Aparência", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 10,
                        Children =
                        {
                            new Label { Text = "Animações", VerticalOptions = LayoutOptions.Center },
                            _animationsSwitch
                        }
                    },
                    new Label { Text = "Observação: desabilitar animações pode melhorar acessibilidade e desempenho.", FontSize = 12, TextColor = Colors.Gray }
                }
            };

            Content = layout;
        }

        private void OnAnimationsToggled(object sender, ToggledEventArgs e)
        {
            try
            {
                if (Application.Current?.Resources != null)
                {
                    Application.Current.Resources["AnimationsEnabled"] = e.Value;
                }
            }
            catch { }
        }
    }
}
