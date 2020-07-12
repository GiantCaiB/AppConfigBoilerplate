using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AppConfigBoilerplate
{
    public class AppConfig
    {
        private static AppConfig _current = BuildAppConfig();

        public static AppConfig Current
        {
            get => _current;
            private set => _current = value;
        }

        public static OnAppConfigReloadDelegate OnAppConfigReload
                = new OnAppConfigReloadDelegate(new Action<AppConfig>((newConfig) => { })); // initialize

        public delegate void OnAppConfigReloadDelegate(AppConfig newConfig);

        private AppConfig() { }

        public bool IsDev
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                // imply configuration not started yet(during deserializing) or app misconfigured / server misconfigured, default to dev
                if (DevEnvIndicators == null || string.IsNullOrEmpty(env))
                {
                    return true; 
                }

                return !DevEnvIndicators
                        .Any(i => env.Contains(i, StringComparison.OrdinalIgnoreCase));
            }
        }

        public string[] DevEnvIndicators { get; set; } = new string[0];

        private static IConfigurationRoot _configurationRoot;

        public static IConfigurationRoot ConfigurationRoot => _configurationRoot ?? (_configurationRoot = BuildConfigurationRoot());

        private static IConfigurationRoot BuildConfigurationRoot()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var appSettingJsonFile = Path.Combine(basePath, "appsettings.json");

            if (!File.Exists(appSettingJsonFile))
            {
                throw new FileNotFoundException("appsettings.json not found in assembly folder.");
            }

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true,
                    reloadOnChange: true);

            return configBuilder.Build();
        }

        private static AppConfig BuildAppConfig()
        {
            var appConfig = new AppConfig();

            ConfigurationRoot.Bind(appConfig);

            ConfigurationRoot
                .GetReloadToken()
                    .RegisterChangeCallback(
                        (state) =>
                        {
                            Task
                            .Delay(5000) // debounce
                            .ContinueWith((t, o) =>
                            {
                                var config = BuildAppConfig();
                                AppConfig.Current = config;
                                OnAppConfigReload(config);
                            }, null);
                        },
                        null);

            return appConfig;
        }

        public class StringRegexCollection : ICollection<string> // to cope with the value binding logic of Microsoft.Extensions
        {
            private HashSet<string> _regexString = new HashSet<string>();
            public List<Regex> Regexes { get; } = new List<Regex>();

            public int Count => _regexString.Count;

            public bool IsReadOnly => false;

            public void Add(string item)
            {
                if (!_regexString.Contains(item))
                {
                    Regexes.Add(new Regex(item));
                    _regexString.Add(item);
                }
            }

            public void Clear()
            {
                _regexString.Clear();
                Regexes.Clear();
            }

            public bool Contains(string item)
            {
                return _regexString.Contains(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                _regexString.CopyTo(array, arrayIndex);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return _regexString.GetEnumerator();
            }

            public bool Remove(string item)
            {
                Regexes.RemoveAll(r => r.ToString() == item);
                return _regexString.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _regexString.GetEnumerator();
            }
        }

        // Add config properties here:
        public int PortNumber {get; set;}
    }
}