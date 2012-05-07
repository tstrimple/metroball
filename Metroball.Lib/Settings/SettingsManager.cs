using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace Metroball.Lib.Settings
{
    public static class SettingsManager
    {
        private const string UserIdKey = "UserId";

        public static void SaveSetting(string key, object value)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);    
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
            }

            IsolatedStorageSettings.ApplicationSettings.Save();    
        }

        public static object GetSetting(string key, object defaultValue)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                SaveSetting(key, defaultValue);
                return defaultValue;
            }

            return IsolatedStorageSettings.ApplicationSettings[key];
        }

        public static string UserId
        {
            get { return (string) GetSetting("UserId", Guid.NewGuid().ToString()); }
            set { SaveSetting("UserId", value);}
        }

        public static bool SoundEnabled
        {
            get { return (bool)GetSetting("SoundEnabled", true); }
            set { SaveSetting("SoundEnabled", value); }
        }

        public static bool DisplayTopPlayers
        {
            get { return (bool)GetSetting("DisplayTopPlayers", false); }
            set { SaveSetting("DisplayTopPlayers", value); }
        }

        public static string Name
        {
            get { return (string) GetSetting("Name", ""); }
            set { SaveSetting("Name", value); }
        }

        public static string ApplicationId
        {
            get { return "00000000-0000-0000-0000-000000000000"; }
        }

        public static string LargeAdId
        {
            get { return "00000"; }
        }

        public static string SmallAdId
        {
            get { return "00000"; }
        }
    }
}
