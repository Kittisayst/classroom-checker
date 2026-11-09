using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace ClassroomChecker.Config
{
    /// <summary>
    /// ບໍລິການຈັດການອ່ານ ແລະ ບັນທຶກ config.json ແບບ portable ພ້ອມ fallback ປອດໄພ
    /// </summary>
    public class ConfigService
    {
        private static ConfigService _instance;
        private static readonly object _lock = new object();

        private readonly string _primaryFilePath;
        private readonly string _fallbackFilePath;
        private string _activeFilePath;
        private bool _isUsingFallback;

        private AppConfig _currentConfig;
        private readonly JavaScriptSerializer _serializer;

        public static ConfigService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigService();
                        }
                    }
                }
                return _instance;
            }
        }

        public AppConfig Current
        {
            get { return _currentConfig; }
        }

        public string ActiveConfigPath
        {
            get { return _activeFilePath; }
        }

        public bool IsUsingFallback
        {
            get { return _isUsingFallback; }
        }

        private ConfigService()
        {
            _serializer = new JavaScriptSerializer();

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _primaryFilePath = Path.Combine(appDir, "config.json");

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _fallbackFilePath = Path.Combine(appData, "ClassroomChecker", "config.json");

            _activeFilePath = _primaryFilePath;
            _isUsingFallback = false;

            Load();
        }

        /// <summary>
        /// ໂຫຼດການຕັ້ງຄ່າຈາກ config.json, ຖ້າບໍ່ມີ ຫຼື ເສຍ ຈະສ້າງໃໝ່
        /// </summary>
        public void Load()
        {
            lock (_lock)
            {
                // ກວດສອບສິດການຂຽນໃນໂຟນເດີ exe (ປ້ອງກັນ USB read-only)
                _activeFilePath = DetermineBestConfigPath();

                if (!File.Exists(_activeFilePath))
                {
                    _currentConfig = new AppConfig();
                    Save();
                    return;
                }

                try
                {
                    string json = File.ReadAllText(_activeFilePath, Encoding.UTF8);
                    _currentConfig = _serializer.Deserialize<AppConfig>(json);

                    if (_currentConfig == null)
                    {
                        throw new InvalidDataException("JSON deserialize returned null");
                    }

                    // ກວດສອບຄ່າ default ຖ້າມີ field ທີ່ຂາດຫາຍ
                    EnsureConfigDefaults(_currentConfig);
                }
                catch (Exception ex)
                {
                    // ຖ້າ JSON ເສຍຫາຍ: backup ເປັນ config.json.bad ແລ້ວສ້າງໃໝ່, ຫ້າມ crash!
                    System.Diagnostics.Debug.WriteLine("Config corrupt, backing up: " + ex.Message);
                    try
                    {
                        string badPath = _activeFilePath + ".bad";
                        if (File.Exists(_activeFilePath))
                        {
                            File.Copy(_activeFilePath, badPath, true);
                        }
                    }
                    catch { }

                    _currentConfig = new AppConfig();
                    Save();
                }
            }
        }

        /// <summary>
        /// ບັນທຶກການຕັ້ງຄ່າລົງ config.json
        /// </summary>
        public bool Save()
        {
            lock (_lock)
            {
                if (_currentConfig == null)
                {
                    _currentConfig = new AppConfig();
                }

                try
                {
                    string dir = Path.GetDirectoryName(_activeFilePath);
                    if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string json = _serializer.Serialize(_currentConfig);
                    // ຈັດ format ງ່າຍໆ
                    json = FormatJson(json);

                    File.WriteAllText(_activeFilePath, json, Encoding.UTF8);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to save config to active path: " + ex.Message);

                    // ຖ້າຂຽນລົງ primary ບໍ່ໄດ້ (ເຊັ່ນ USB write-protected) ໃຫ້ fallback ໄປ AppData
                    if (!_isUsingFallback)
                    {
                        try
                        {
                            _isUsingFallback = true;
                            _activeFilePath = _fallbackFilePath;

                            string fallbackDir = Path.GetDirectoryName(_fallbackFilePath);
                            if (!Directory.Exists(fallbackDir))
                            {
                                Directory.CreateDirectory(fallbackDir);
                            }

                            string json = FormatJson(_serializer.Serialize(_currentConfig));
                            File.WriteAllText(_activeFilePath, json, Encoding.UTF8);
                            return true;
                        }
                        catch { }
                    }
                    return false;
                }
            }
        }

        private string DetermineBestConfigPath()
        {
            // ກວດສອບວ່າ primary directory ສາມາດຂຽນໄດ້ຫຼືບໍ່
            string primaryDir = Path.GetDirectoryName(_primaryFilePath);
            if (CanWriteToDirectory(primaryDir))
            {
                _isUsingFallback = false;
                return _primaryFilePath;
            }

            // ຖ້າຂຽນບໍ່ໄດ້ (USB read-only), ໃຊ້ AppData
            _isUsingFallback = true;
            return _fallbackFilePath;
        }

        private bool CanWriteToDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                return false;
            }

            string testFile = Path.Combine(directoryPath, ".write_test_" + Guid.NewGuid().ToString("N"));
            try
            {
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureConfigDefaults(AppConfig cfg)
        {
            if (cfg.rooms == null || cfg.rooms.Count == 0)
            {
                cfg.rooms = new System.Collections.Generic.List<string> {
                    "ຫ້ອງຄອມພິວເຕີ 1",
                    "ຫ້ອງຄອມພິວເຕີ 2",
                    "ຫ້ອງຄອມພິວເຕີ 3",
                    "ຫ້ອງຄອມພິວເຕີ 4",
                    "ຫ້ອງຄອມພິວເຕີ 5",
                    "ຫ້ອງຄອມພິວເຕີ 6",
                    "ຫ້ອງຄອມພິວເຕີ 7",
                    "ຫ້ອງຄອມພິວເຕີ 8"
                };
            }
            if (cfg.computerNumberRange == null)
            {
                cfg.computerNumberRange = new RangeConfig();
            }
            if (cfg.selected == null)
            {
                cfg.selected = new SelectedConfig();
            }
            if (cfg.background == null)
            {
                cfg.background = new BackgroundConfig();
            }
            if (cfg.label == null)
            {
                cfg.label = new LabelConfig();
            }
            if (cfg.cleanup == null)
            {
                cfg.cleanup = new CleanupConfig();
            }
            if (cfg.security == null)
            {
                cfg.security = new SecurityConfig();
            }
            if (string.IsNullOrEmpty(cfg.language))
            {
                cfg.language = "lo";
            }
        }

        private string FormatJson(string json)
        {
            // ຈັດ Indent ງ່າຍໆ ເພື່ອໃຫ້ຜູ້ໃຊ້ເປີດແກ້ໃນ notepad ໄດ້ງ່າຍ
            StringBuilder sb = new StringBuilder();
            bool insideQuote = false;
            int indent = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    insideQuote = !insideQuote;
                }

                if (!insideQuote)
                {
                    if (c == '{' || c == '[')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                        continue;
                    }
                    if (c == '}' || c == ']')
                    {
                        sb.Append("\r\n");
                        indent = Math.Max(0, indent - 1);
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(c);
                        continue;
                    }
                    if (c == ',')
                    {
                        sb.Append(c);
                        sb.Append("\r\n");
                        sb.Append(new string(' ', indent * 2));
                        continue;
                    }
                    if (c == ':')
                    {
                        sb.Append(": ");
                        continue;
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
