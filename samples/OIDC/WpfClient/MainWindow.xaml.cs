using System;
using System.Collections.Generic;
using System.Windows;
using Thinktecture.IdentityModel.Client;
using Thinktecture.Samples;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LoginWebView _login;

        public MainWindow()
        {
            InitializeComponent();

            _login = new LoginWebView();
            _login.Done += _login_Done;

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _login.Owner = this;
        }

        void _login_Done(object sender, AuthorizeResponse e)
        {
            Textbox1.Text = e.Raw;
        }

        private void LoginOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            var additional = new Dictionary<string, string>
            {
                { "nonce", "nonce" }
            };

            var client = new OAuth2Client(new Uri("http://localhost:3333/core/connect/authorize"));
            var startUrl = client.CreateAuthorizeUrl(
                "implicitclient",
                "id_token",
                "openid",
                "oob://localhost/wpfclient",
                "state",
                additional);
                

            _login.Show();
            _login.Start(new Uri(startUrl), new Uri("oob://localhost/wpfclient"));
        }
    }
}