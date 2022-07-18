using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel;
using static anytunes.Services.EvitunesService;
using System.Collections.ObjectModel;
using System.ComponentModel;
using anytunes.ViewModels;

namespace anytunes;

public partial class Player : ContentPage
{
    public readonly IOSoapChannel _proxy;
    private readonly HttpRequestMessageProperty _requestHeader;
    private readonly string _authToken;
    private ObservableCollection<SongDataItem> albumSongsResponse { get; set; } = new ObservableCollection<SongDataItem>();


    public Player(SelectedAlbumDataItem searchResults)
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

        albumSongsResponse = new ObservableCollection<SongDataItem>(searchResults.SongsData);
        lstAlbumSongs.ItemsSource = albumSongsResponse;

        //lstArtists.ItemsSource = artistSearchResponse;
        //BindingContext = this;
    }

    private void GetSongsByArtistId(int Id, string ArtistName)
    {
        using (OperationContextScope scope = new OperationContextScope(_proxy))
        {
            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = _requestHeader;

            //_isMainSearchRunning = true;
            //actMainSearch.IsRunning = true;
            //await System.Threading.Tasks.Task.Delay(30);

            lstAlbumSongs.BeginRefresh();

            var response = new ObservableCollection<SongDataItem>((_proxy.GetArtistsOrSoundTrackSongsByIdAsync(_authToken, Id).Result ?? new SongDataItem[] { })
                .ToList()
                .OrderBy(tr => tr.SongNumber)
                .OrderBy(tr => tr.CDNumber)
                .OrderBy(tr => tr.ArtistOrSoundTrackName).ToList());

            albumSongsResponse = response;
            lstAlbumSongs.ItemsSource = albumSongsResponse;

            lstAlbumSongs.EndRefresh();

            //await System.Threading.Tasks.Task.Delay(30);
            //_isMainSearchRunning = false;
            //actMainSearch.IsRunning = false;
        }
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

    private async void OnbtnHeaderBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}