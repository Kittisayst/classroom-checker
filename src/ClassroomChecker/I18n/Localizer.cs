using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using ClassroomChecker.Config;

namespace ClassroomChecker.I18n
{
    /// <summary>
    /// ລະບົບຈັດການສອງພາສາ (ລາວ / English) ຜ່ານໄຟລ໌ JSON ທີ່ຝັງເປັນ EmbeddedResource
    /// </summary>
    public static class Localizer
    {
        private static Dictionary<string, string> _strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string _currentLanguage = "lo";
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public static event Action LanguageChanged;

        public static string CurrentLanguage
        {
            get { return _currentLanguage; }
        }

        static Localizer()
        {
            // ອ່ານພາສາເລີ່ມຕົ້ນຈາກ Config
            string initialLang = ConfigService.Instance.Current.language;
            if (string.IsNullOrEmpty(initialLang))
            {
                initialLang = "lo";
            }
            SetLanguage(initialLang);
        }

        /// <summary>
        /// ປ່ຽນພາສາ ("lo" ຫຼື "en")
        /// </summary>
        public static void SetLanguage(string langCode)
        {
            if (string.IsNullOrEmpty(langCode))
            {
                langCode = "lo";
            }
            langCode = langCode.ToLowerInvariant();
            if (langCode != "lo" && langCode != "en")
            {
                langCode = "lo";
            }

            _currentLanguage = langCode;
            LoadLanguageStrings(langCode);

            // ອັບເດດລົງ Config ທັນທີ
            if (ConfigService.Instance.Current.language != langCode)
            {
                ConfigService.Instance.Current.language = langCode;
                ConfigService.Instance.Save();
            }

            if (LanguageChanged != null)
            {
                LanguageChanged.Invoke();
            }
        }

        /// <summary>
        /// ແປຂໍ້ຄວາມຕາມ key ທີ່ກຳນົດ ພ້ອມ format placeholder ({0}, {1}, ...)
        /// </summary>
        public static string T(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            string value;
            if (!_strings.TryGetValue(key, out value) || string.IsNullOrEmpty(value))
            {
                // Fallback: ສົ່ງຄືນຊື່ key ຖ້າບໍ່ພົບ
                value = key;
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(value, args);
                }
                catch
                {
                    return value;
                }
            }

            return value;
        }

        private static void LoadLanguageStrings(string langCode)
        {
            _strings.Clear();

            // 1. ລອງອ່ານຈາກໄຟລ໌ຂ້າງນອກ (ຖ້າມີ)
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string externalPath = Path.Combine(appDir, "I18n", langCode + ".json");
            if (File.Exists(externalPath))
            {
                try
                {
                    string json = File.ReadAllText(externalPath, Encoding.UTF8);
                    _strings = _serializer.Deserialize<Dictionary<string, string>>(json);
                    if (_strings != null && _strings.Count > 0)
                    {
                        return;
                    }
                }
                catch { }
            }

            // 2. ໂຫຼດຈາກ EmbeddedResource
            Assembly asm = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("ClassroomChecker.I18n.{0}.json", langCode);

            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        _strings = _serializer.Deserialize<Dictionary<string, string>>(json);
                    }
                }
            }

            if (_strings == null)
            {
                _strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
