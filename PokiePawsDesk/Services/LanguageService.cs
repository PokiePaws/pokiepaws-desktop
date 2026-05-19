using System;
using System.Linq;
using System.Windows;

namespace PokiePawsDesk.Services
{
    public class LanguageService
    {
        private static readonly LanguageService _instance = new();
        public static LanguageService Instance => _instance;

        public string CurrentLanguage { get; private set; } = "pl";

        public void SetLanguage(string lang)
        {
            CurrentLanguage = lang;
            var uri = new Uri($"Resources/Strings.{lang}.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };

            var existing = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));
            if (existing != null)
                Application.Current.Resources.MergedDictionaries.Remove(existing);

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        public static string Get(string key)
        {
            return Application.Current?.Resources[key] as string ?? key;
        }
    }
}