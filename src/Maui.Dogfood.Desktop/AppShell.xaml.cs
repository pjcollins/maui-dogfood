namespace Maui.Dogfood.Desktop;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        BindingContext = this;
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(OutputPage), typeof(OutputPage));
    }

    string _selectedRoute = string.Empty;
    public string SelectedRoute
    {
        get {
            return _selectedRoute;
        }
        set {
            _selectedRoute = value;
            OnPropertyChanged();
        }
    }

    async void OnMenuItemChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedRoute))
            await Current.GoToAsync($"//{SelectedRoute}");
    }
}

