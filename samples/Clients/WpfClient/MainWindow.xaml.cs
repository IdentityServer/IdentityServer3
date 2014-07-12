using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        AuthorizeResponse _response;

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
            _response = e;
            Textbox1.Text = e.Raw;
        }

        private void LoginOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("openid", "id_token");
        }

        private void LoginWithProfileButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("openid profile", "id_token");
        }

        private void LoginWithProfileAndAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("openid profile read write", "id_token token");
        }

        private void AccessTokenOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            RequestToken("read write", "token");
        }

        private void RequestToken(string scope, string responseType)
        {
            var additional = new Dictionary<string, string>
            {
                { "nonce", "nonce" },
                // { "login_hint", "idp:Google" }
            };

            var client = new OAuth2Client(new Uri(Constants.AuthorizeEndpoint));
            var startUrl = client.CreateAuthorizeUrl(
                "implicitclient",
                responseType,
                scope,
                "oob://localhost/wpfclient",
                "state",
                additional);


            _login.Show();
            _login.Start(new Uri(startUrl), new Uri("oob://localhost/wpfclient"));
        }

        private void ShowIdTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("id_token"))
            {
                var viewer = new IdentityTokenViewer();
                viewer.IdToken = _response.Values["id_token"];
                viewer.Show();
            }
        }

        private void ShowAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("access_token"))
            {
                var viewer = new IdentityTokenViewer();
                viewer.IdToken = _response.Values["access_token"];
                viewer.Show();
            }
        }

        private async void CallServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_response.Values.ContainsKey("access_token"))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:2727/")
                };
                client.SetBearerToken(_response.AccessToken);

                var response = await client.GetAsync("identity");
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Textbox1.Text = JArray.Parse(json).ToString();
                }
                else
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
            }
        }
    }
}