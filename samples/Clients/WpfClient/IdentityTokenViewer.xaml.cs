using Newtonsoft.Json.Linq;
using System.Text;
using System.Windows;
using Thinktecture.IdentityModel.Client;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for IdentityTokenViewer.xaml
    /// </summary>
    public partial class IdentityTokenViewer : Window
    {
        public IdentityTokenViewer()
        {
            InitializeComponent();
        }

        public string IdToken
        {
            set
            {
                var token = value;
                var parts = token.Split('.');
                var part = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));

                var jwt = JObject.Parse(part);
                Text.Text = jwt.ToString();
            }
        }
    }
}
