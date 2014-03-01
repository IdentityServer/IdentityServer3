using System;
using System.Windows;
using System.Windows.Navigation;
using Thinktecture.IdentityModel.Client;

namespace Thinktecture.Samples
{
    public partial class LoginWebView : Window
    {
        public AuthorizeResponse AuthorizeResponse { get; set; }
        public event EventHandler<AuthorizeResponse> Done;

        Uri _callbackUri;

        public LoginWebView()
        {
            InitializeComponent();
            webView.Navigating += webView_Navigating;

            Closing += LoginWebView_Closing;
        }

        void LoginWebView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        public void Start(Uri startUri, Uri callbackUri)
        {
            _callbackUri = callbackUri;
            webView.Navigate(startUri);
        }

        private void webView_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(_callbackUri.AbsoluteUri))
            {
                AuthorizeResponse = new AuthorizeResponse(e.Uri.AbsoluteUri);

                e.Cancel = true;
                this.Visibility = System.Windows.Visibility.Hidden;
                
                if (Done != null)
                {
                    Done.Invoke(this, AuthorizeResponse);
                }
            }
        }
    }
}