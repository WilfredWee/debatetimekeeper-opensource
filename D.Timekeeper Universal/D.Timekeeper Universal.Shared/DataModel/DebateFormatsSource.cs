using D.Timekeeper_Universal.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace D.Timekeeper_Universal.DataModel
{
    public sealed class DebateFormatsSource
    {
        private static DebateFormatsSource _debateFormatsSource = new DebateFormatsSource();

        private ObservableCollection<DebateFormat> _debateFormats = new ObservableCollection<DebateFormat>();
        public ObservableCollection<DebateFormat> DebateFormats
        {
            get { return this._debateFormats; }
        }

        public static async Task<IEnumerable<DebateFormat>> GetDebateFormatsAsync()
        {
            await _debateFormatsSource.GetDebateFormatsDataAsync();
            return _debateFormatsSource.DebateFormats;
        }

        public static async Task SaveNewDebateFormat(DebateFormat df)
        {
            await _debateFormatsSource.GetDebateFormatsDataAsync();
            var debateFormats = _debateFormatsSource.DebateFormats;
            debateFormats.Add(df);

            await WriteJSONToFile(debateFormats);

            
        }

        public static async Task<IEnumerable<DebateFormat>> DeleteDebateFormat(DebateFormat df)
        {
            await _debateFormatsSource.GetDebateFormatsDataAsync();
            var debateFormats = _debateFormatsSource.DebateFormats;

            foreach(DebateFormat dFormat in debateFormats)
            {
                if(dFormat.name == df.name)
                {
                    debateFormats.Remove(dFormat);
                    break;
                }
            }
            await WriteJSONToFile(debateFormats);

            return debateFormats;
        }

        private static async Task WriteJSONToFile(ObservableCollection<DebateFormat> debateFormats)
        {
            string newJsonString = JsonConvert.SerializeObject(debateFormats, Formatting.Indented);

            Uri dataUri = new Uri("ms-appx:///DataModel/DebateFormats.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);

            await FileIO.WriteTextAsync(file, newJsonString);
        }

        private async Task GetDebateFormatsDataAsync()
        {
            if (this._debateFormats.Count != 0)
            {
                return;
            }

            Uri dataUri = new Uri("ms-appx:///DataModel/DebateFormats.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);

            this._debateFormats = new ObservableCollection<DebateFormat>(JsonConvert.DeserializeObject<List<DebateFormat>>(jsonText));
        }

        private int CompareKeys(KeyValuePair<int, int> x, KeyValuePair<int, int> y)
        {
            return x.Key.CompareTo(y.Key);
        }
    }
}
