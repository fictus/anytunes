using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using static anytunes.Services.EvitunesService;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace anytunes;

public partial class Player : ContentPage
{
    public readonly IOSoapChannel _proxy;
    private readonly HttpRequestMessageProperty _requestHeader;
    private readonly string _authToken;
    private bool _isMainSearchRunning = false;
    private ObservableCollection<ArtistOrSoundTrackDataItem> artistSearchResponse { get; set; } = new ObservableCollection<ArtistOrSoundTrackDataItem>();


    public Player()
	{
        _proxy = (new MainPage())._proxy;
        _authToken = MainPage._authToken;
        _requestHeader = new HttpRequestMessageProperty()
        {
            Headers =
                {
                    { "UserHostAddress", MainPage._ipAddress },
                    { HttpRequestHeader.UserAgent, $"anytunes {DeviceInfo.Platform} {DeviceInfo.VersionString} User Agent"}
                }
        };

        InitializeComponent();

        //lstArtists.ItemsSource = artistSearchResponse;
        //BindingContext = this;
    }

    private void OnRdbSelectorSearchChanged(object sender, EventArgs e)
	{
        stkSearchControls.IsVisible = true;
        lstArtists.IsVisible = true;
    }

    private void OnRdbSelectorPlaylistChanged(object sender, EventArgs e)
	{
        stkSearchControls.IsVisible = false;
        lstArtists.IsVisible = false;
    }

    private void OnRdbSelectorShuffleChanged(object sender, EventArgs e)
	{
        stkSearchControls.IsVisible = false;
        lstArtists.IsVisible = false;
    }

    private void OnSearchEnterPresses(object sender, EventArgs e)
    {
        OnBtnMainSearchClicked(sender, e);
    }

    private async void OnBtnMainSearchClicked(object sender, EventArgs e)
	{
		if (_isMainSearchRunning)
		{
			return;
		}

        //if (string.IsNullOrWhiteSpace(txtMainSearch.Text))
        //{
        //    return;
        //}

		_isMainSearchRunning = true;
        actMainSearch.IsRunning = true;
        await System.Threading.Tasks.Task.Delay(30);

        using (OperationContextScope scope = new OperationContextScope(_proxy))
        {
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = _requestHeader;

            //var sendPing = _glabals._proxy.PingAsync(MainPage._authToken).Result;
            var response = _proxy.GetEvitunesSearchAsync(new Services.EvitunesService.GetEvitunesSearchRequest()
            {
                isArtist = cmbSearchType.SelectedIndex == 0 ? "1" : "0",
                ArtistOrSongName = txtMainSearch.Text,
                Token = _authToken
            }).Result;

            if (cmbSearchType.SelectedIndex == 0)
            {
                lstArtists.BeginRefresh();

                artistSearchResponse = new ObservableCollection<ArtistOrSoundTrackDataItem>((response.GetEvitunesSearchResult.ArtistData ?? new ArtistOrSoundTrackDataItem[] { }).ToList());
                lstArtists.ItemsSource = artistSearchResponse;

                lstArtists.EndRefresh();
            }
            else
            {

            }

            await System.Threading.Tasks.Task.Delay(30);
            _isMainSearchRunning = false;
            actMainSearch.IsRunning = false;
        }
    }

    private void lstArtistsRowTapped(object sender, EventArgs e)
    {
        var grid = (Grid)sender;
        var lblName = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Name").FirstOrDefault();
        var lblId = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Id").FirstOrDefault();        
        
    }
}