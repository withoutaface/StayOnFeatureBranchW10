using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace StayOnFeatureBranchW10
{
    class JsonConfig
    {
        Windows10FeatureUpdateInfo _currentFeatureUpdate { get; set; }

        List<Windows10FeatureUpdateInfo> _configFeatureUpdates { get; set; }

        string _startUpPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string _configFileFeatureUpdates;

        public Windows10FeatureUpdateInfo GetCurrentFeatureUpdate(string version)
        {
            if(_currentFeatureUpdate.version == null)
            {
                foreach (Windows10FeatureUpdateInfo info in _configFeatureUpdates)
                {
                    if (info.version == version)
                    {
                        _currentFeatureUpdate = info;
                    }
                }

                return _currentFeatureUpdate;
            }
            else
            {
                return _currentFeatureUpdate;
            }
        }

        public bool GetFeatureUpdateEndofSupport(Windows10FeatureUpdateInfo update)
        {
            if (update.version != null)
            {
                string[] datearray = update.endofsupport.Split('-');
                DateTime endofsupport = new DateTime(int.Parse(datearray[0]), int.Parse(datearray[1]), int.Parse(datearray[2]));

                if (DateTime.Now >= endofsupport)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Windows10FeatureUpdateInfo GetRecommendedFeatureUpdate()
        {
            if(_currentFeatureUpdate.version != null)
            {
                foreach (Windows10FeatureUpdateInfo info in _configFeatureUpdates)
                {
                    if (info.rank > _currentFeatureUpdate.rank && info.recommended && !GetFeatureUpdateEndofSupport(info))
                    {
                        return info;
                    }
                }
                
            }
            else
            {
                return _configFeatureUpdates[0];
            }

            return _currentFeatureUpdate;
        }

        public List<Windows10FeatureUpdateInfo> GetFeatureUpdates()
        {
            return _configFeatureUpdates;
        }

        public JsonConfig()
        {
            _configFeatureUpdates = new List<Windows10FeatureUpdateInfo>();
            _configFileFeatureUpdates = _startUpPath + "\\featureupdates.json";
        }

        public void LoadFeatureUpdateConfigFromJson()
        {
            string configFeatureUpdatesAsJSON;

            using (StreamReader sr = File.OpenText(_configFileFeatureUpdates))
            {
                configFeatureUpdatesAsJSON = sr.ReadToEnd();
            }

            _configFeatureUpdates = JsonConvert.DeserializeObject<List<Windows10FeatureUpdateInfo>>(configFeatureUpdatesAsJSON);

        }

        public string SaveFeatureUpdateConfigToJson()
        {
            //List of feature updates
            Windows10FeatureUpdateInfo update1507 = new Windows10FeatureUpdateInfo { rank = 1, version = "1507", build = "10240", name = "Release to Manufacturing (RTM) (Threshold 1)", releasedate = "2015-07-29", endofsupport = "2017-05-09", recommended = true };
            Windows10FeatureUpdateInfo update1511 = new Windows10FeatureUpdateInfo { rank = 2, version = "1511", build = "10586", name = "November Update (Threshold 2)", releasedate = "2015-11-10", endofsupport = "2017-10-10", recommended = false };
            Windows10FeatureUpdateInfo update1607 = new Windows10FeatureUpdateInfo { rank = 3, version = "1607", build = "14393", name = "Anniversary Update (Redstone 1)", releasedate = "2016-08-02", endofsupport = "2018-04-10", recommended = true };
            Windows10FeatureUpdateInfo update1703 = new Windows10FeatureUpdateInfo { rank = 4, version = "1703", build = "15063", name = "Creators Update (Redstone 2)", releasedate = "2017-04-05", endofsupport = "2018-10-09", recommended = false };
            Windows10FeatureUpdateInfo update1709 = new Windows10FeatureUpdateInfo { rank = 5, version = "1709", build = "16299", name = "Fall Creators Update (Redstone 3)", releasedate = "2017-10-17", endofsupport = "2019-04-09", recommended = false };
            Windows10FeatureUpdateInfo update1803 = new Windows10FeatureUpdateInfo { rank = 6, version = "1803", build = "17134", name = "April-2018-Update (Redstone 4)", releasedate = "2018-04-30", endofsupport = "2019-11-12", recommended = false };
            Windows10FeatureUpdateInfo update1809 = new Windows10FeatureUpdateInfo { rank = 7, version = "1809", build = "17763", name = "Oktober-2018-Update (Redstone 5)", releasedate = "2018-10-02", endofsupport = "2020-11-10", recommended = true };
            Windows10FeatureUpdateInfo update1903 = new Windows10FeatureUpdateInfo { rank = 8, version = "1903", build = "18362", name = "Mai-2019-Update (19H1)", releasedate = "2019-05-21", endofsupport = "2020-12-08", recommended = false };
            Windows10FeatureUpdateInfo update1909 = new Windows10FeatureUpdateInfo { rank = 9, version = "1909", build = "18363", name = "November-2019-Update (19H2)", releasedate = "2019-11-12", endofsupport = "2021-05-11", recommended = true };
            Windows10FeatureUpdateInfo update2004 = new Windows10FeatureUpdateInfo { rank = 10, version = "2004", build = "19041", name = "April-2020-Update (20H1)", releasedate = "2020-05-27", endofsupport = "2021-12-14", recommended = false };
            Windows10FeatureUpdateInfo update20H2 = new Windows10FeatureUpdateInfo { rank = 11, version = "20H2", build = "19042", name = "Oktober-2020-Update (20H2)", releasedate = "2020-10-20", endofsupport = "2022-05-10", recommended = true };
            Windows10FeatureUpdateInfo update21H1 = new Windows10FeatureUpdateInfo { rank = 12, version = "21H1", build = "19043", name = "Mai-2021-Update (21H1)", releasedate = "2021-05-18", endofsupport = "2022-12-13", recommended = false };
            Windows10FeatureUpdateInfo update21H2 = new Windows10FeatureUpdateInfo { rank = 13, version = "21H2", build = "19044", name = "November-2021-Update (21H2)", releasedate = "2021-11-16", endofsupport = "2023-06-13", recommended = true };

            _configFeatureUpdates = new List<Windows10FeatureUpdateInfo>();
            _configFeatureUpdates.Add(update1507);
            _configFeatureUpdates.Add(update1511);
            _configFeatureUpdates.Add(update1607);
            _configFeatureUpdates.Add(update1703);
            _configFeatureUpdates.Add(update1709);
            _configFeatureUpdates.Add(update1803);
            _configFeatureUpdates.Add(update1809);
            _configFeatureUpdates.Add(update1903);
            _configFeatureUpdates.Add(update1909);
            _configFeatureUpdates.Add(update2004);
            _configFeatureUpdates.Add(update20H2);
            _configFeatureUpdates.Add(update21H1);
            _configFeatureUpdates.Add(update21H2);

            string configFeatureUpdatesAsJSON = JsonConvert.SerializeObject(_configFeatureUpdates, Formatting.Indented);

            //Delete json file if exists
            if(File.Exists(_configFileFeatureUpdates))
            {
                File.Delete(_configFileFeatureUpdates);
            }

            //Save to file
            using (StreamWriter sw = File.CreateText(_configFileFeatureUpdates))
            {
                sw.Write(configFeatureUpdatesAsJSON);
            }

            return configFeatureUpdatesAsJSON;
        }

        public bool UserIsAdmin()
        {
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

    }

    public struct Windows10FeatureUpdateInfo
    {
        public int rank;
        public string version;
        public string build;
        public string name;
        public string releasedate;
        public string endofsupport;
        public bool recommended;
    }
}
