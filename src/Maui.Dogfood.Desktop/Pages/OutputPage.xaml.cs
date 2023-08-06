using System;

namespace Maui.Dogfood.Desktop;

public partial class OutputPage : ContentPage
{
    public OutputPage()
    {
        InitializeComponent();
    }

    void OutputButton_Clicked(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.Dispatch(() => OutputEditor.Text = Logger.ReadLogFile());
    }
}

