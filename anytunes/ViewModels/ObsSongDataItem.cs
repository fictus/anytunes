using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anytunes.ViewModels
{
    public class ObsSongDataItem
    {
        public string SongId { get; set; }
        public int SongNumber { get; set; }
        public int CDNumber { get; set; }
        public string SongName { get; set; }
        public string Notes { get; set; }
        public bool IsPlaying { get; set; }
    }
}
