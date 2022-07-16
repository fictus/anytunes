//using AndroidX.ConstraintLayout.Helper.Widget;
using Microsoft.Extensions.Options;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using static anytunes.Services.EvitunesService;

namespace anytunes;

public partial class MainPage : ContentPage
{
    public readonly IOSoapChannel _proxy;
    public static string _ipAddress;
    public static string _authToken;
    private bool _areButtonsEnabled = true;

    public MainPage()
	{
        /*
         *Create & Configure Client
         */
        BasicHttpBinding binding = new BasicHttpBinding
        {
            SendTimeout = TimeSpan.FromSeconds(18),
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true,
            ReaderQuotas = XmlDictionaryReaderQuotas.Max
        };

        binding.Security.Mode = BasicHttpSecurityMode.Transport;
        EndpointAddress address = new EndpointAddress("https://evicore.net/evitunesService/IO.asmx");
        
        ChannelFactory<IOSoapChannel> factory = new ChannelFactory<IOSoapChannel>(binding, address);
        this._proxy = factory.CreateChannel();

        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                _ipAddress = ip.ToString();
            }
        }

        InitializeComponent();
	}

    private async void OnbtnLoginClicked(object sender, EventArgs e)
	{
        if (!_areButtonsEnabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            lblLoginStatus.Text = "Username/Password are required";
            SemanticScreenReader.Announce(lblLoginStatus.Text);

            return;
        }

        lblLoginStatus.Text = "";

        _areButtonsEnabled = false;
        activityRunning.IsRunning = true;
        await System.Threading.Tasks.Task.Delay(30);

        using (OperationContextScope scope = new OperationContextScope(_proxy))
        {
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = new HttpRequestMessageProperty()
            {
                Headers =
                {
                    { "UserHostAddress", _ipAddress },
                    { HttpRequestHeader.UserAgent, $"anytunes {DeviceInfo.Platform} {DeviceInfo.VersionString} User Agent"}
                }
            };

            var response = _proxy.AuthenticateUserAsync(txtUserName.Text.Trim(), txtPassword.Text).Result;

            if (response == null)
            {
                lblLoginStatus.Text = "Username or Password is incorrect";
            }
            else
            {
                if (response.error)
                {
                    lblLoginStatus.Text = "Username or Password is incorrect";
                }
                else
                {
                    _authToken = response.token;

                    App.Current.MainPage = new Player();
                }
            }

            _areButtonsEnabled = true;

            activityRunning.IsRunning = false;
            await System.Threading.Tasks.Task.Delay(30);

            SemanticScreenReader.Announce(lblLoginStatus.Text);
        }
	}

    private void OnbtnOfflineClicked(object sender, EventArgs e)
    {

    }

    public void AddProgressDisplay()
    {
        var content = this.Content;

        var grid = new Grid();
        grid.@class.Add("grdWaitClass");

        grid.Children.Add(content);

        var gridProgress = new Grid { BackgroundColor = Color.FromArgb("#64FFE0B2"), Padding = new Thickness(50) };
        gridProgress.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        gridProgress.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        gridProgress.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        gridProgress.SetBinding(VisualElement.IsVisibleProperty, "IsWorking");

        var activity = new ActivityIndicator
        {
            IsEnabled = true,
            IsVisible = true,
            HorizontalOptions = LayoutOptions.Fill,
            IsRunning = true
        };

        gridProgress.Children.Add(activity);
        grid.Children.Add(gridProgress);
        this.Content = grid;
    }
}

