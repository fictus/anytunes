using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static anytunes.Services.EvitunesService;

namespace anytunes.ViewModels
{
    public class SelectedAlbumDataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SongDataItem> SongsData { get; set; }
    }
}
