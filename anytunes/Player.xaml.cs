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
    private ObservableCollection<SongDataItem> albumSongsResponse { get; set; } = new ObservableCollection<SongDataItem>();


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
        if (_isMainSearchRunning)
        {
            return;
        }

        var grid = (Grid)sender;
        var lblName = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Name").FirstOrDefault();
        var lblId = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Id").FirstOrDefault();

        if (lblName != null && lblId != null && !string.IsNullOrWhiteSpace(((Label)lblId).Text))
        {
            GetSongsByArtistId(Convert.ToInt32(((Label)lblId).Text), ((Label)lblName).Text);
        }
    }

    private async void GetSongsByArtistId(int Id, string ArtistName)
    {
        using (OperationContextScope scope = new OperationContextScope(_proxy))
        {
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = _requestHeader;

            _isMainSearchRunning = true;
            actMainSearch.IsRunning = true;
            await System.Threading.Tasks.Task.Delay(30);

            lstAlbumSongs.BeginRefresh();

            var response = new ObservableCollection<SongDataItem>((_proxy.GetArtistsOrSoundTrackSongsByIdAsync(_authToken, Id).Result ?? new SongDataItem[] { })
                .ToList()
                .OrderBy(tr => tr.SongNumber)
                .OrderBy(tr => tr.CDNumber)
                .OrderBy(tr => tr.ArtistOrSoundTrackName).ToList());

            albumSongsResponse = response;
            lstAlbumSongs.ItemsSource = albumSongsResponse;

            lstAlbumSongs.EndRefresh();

            await System.Threading.Tasks.Task.Delay(30);
            _isMainSearchRunning = false;
            actMainSearch.IsRunning = false;

            ShowAlbumSongs();
        }
    }

    private void ShowAlbumSongs()
    {
        stkMainSearch.IsVisible = false;
        stkSearchControls.IsVisible = false;
        stkArtists.IsVisible = false;

        stkHeaderAlbumSongs.IsVisible = true;
        stkAlbumSongs.IsVisible = true;
    }

    private void ShowMainSearch()
    {
        stkHeaderAlbumSongs.IsVisible = false;
        stkAlbumSongs.IsVisible = false;

        stkMainSearch.IsVisible = true;
        stkSearchControls.IsVisible = true;
        stkArtists.IsVisible = true;
    }

    private void lstAlbumSongsRowTapped(object sender, EventArgs e)
    {
        //if (_isMainSearchRunning)
        //{
        //    return;
        //}

        //var grid = (Grid)sender;
        //var lblName = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Name").FirstOrDefault();
        //var lblId = grid.Children.Where(c => c is Label && ((Label)c).ClassId == "lblArtists_Id").FirstOrDefault();

        //if (lblName != null && lblId != null && !string.IsNullOrWhiteSpace(((Label)lblId).Text))
        //{
        //    GetSongsByArtistId(Convert.ToInt32(((Label)lblId).Text), ((Label)lblName).Text);
        //}
    }

    private void OnbtnHeaderAlbumSongsBackClicked(object sender, EventArgs e)
    {
        ShowMainSearch();
    }
}