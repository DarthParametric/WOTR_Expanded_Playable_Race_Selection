﻿using DP_WOTR_PlayableRaceExp.Config;
using DP_WOTR_PlayableRaceExp.Localization;
using DP_WOTR_PlayableRaceExp.NewEvents;
using Kingmaker.PubSubSystem;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using static UnityModManagerNet.UnityModManager;

namespace DP_WOTR_PlayableRaceExp.ModLogic {
    public abstract class ModContextBase : IBlueprintCacheInitHandler,
        IGlobalSubscriber, ISubscriber {
        public readonly ModEntry ModEntry;
        public readonly ModLogger Logger;
        public string BlueprintsFile = "Blueprints.json";
        public Blueprints Blueprints = new Blueprints();
        public MultiLocalizationPack ModLocalizationPack = new MultiLocalizationPack();
        public virtual string UserConfigFolder => ModEntry.Path + "UserSettings";
        public virtual string LocalizationFolder => ModEntry.Path + "Localization";
        public string LocalizationFile = "LocalizationPack.json";
        public bool Debug;
        private static JsonSerializerSettings cachedSettings;
        private static JsonSerializerSettings SerializerSettings {
            get {
                if (cachedSettings == null) {
                    cachedSettings = new JsonSerializerSettings {
                        CheckAdditionalContent = false,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                        DefaultValueHandling = DefaultValueHandling.Include,
                        FloatParseHandling = FloatParseHandling.Double,
                        Formatting = Formatting.Indented,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        StringEscapeHandling = StringEscapeHandling.Default,
						Culture = CultureInfo.InvariantCulture,
                        Converters = new List<JsonConverter>() { new SortedDictonaryConverter(StringComparer.InvariantCulture) }
                    };
                }
                return cachedSettings;
            }
        }

        public ModContextBase(ModEntry modEntry) {
            Blueprints = new Blueprints();
            ModEntry = modEntry;
            Logger = new ModLogger(ModEntry);
            EventBus.Subscribe(this);
        }

        public abstract void LoadAllSettings();
        public virtual void SaveAllSettings() {
            SaveSettings(BlueprintsFile, Blueprints);
        }
        public virtual void LoadBlueprints(string classPath, ModContextBase context) {
            LoadSettings(BlueprintsFile, classPath, ref Blueprints);
            Blueprints.Context = context;
        }
        public virtual void LoadLocalization(string classPath) {
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"{classPath}.{LocalizationFile}"; ;
            var localizationPath = $"{LocalizationFolder}{Path.DirectorySeparatorChar}{LocalizationFile}";
            Directory.CreateDirectory(LocalizationFolder);
            if (File.Exists(localizationPath)) {
                using (StreamReader streamReader = File.OpenText(localizationPath))
                using (JsonReader jsonReader = new JsonTextReader(streamReader)) {
                    try {
                        MultiLocalizationPack localization = serializer.Deserialize<MultiLocalizationPack>(jsonReader);
                        ModLocalizationPack = localization;
                    } catch {
                        ModLocalizationPack = new MultiLocalizationPack();
                        Logger.LogError("Failed to localization. Settings will be rebuilt.");
                        try { File.Copy(localizationPath, ModEntry.Path + $"{Path.DirectorySeparatorChar}BROKEN_{LocalizationFile}", true); } catch { Logger.LogError("Failed to archive broken localization."); }
                    }
                }
            } else {
                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader)) {
                    ModLocalizationPack = serializer.Deserialize<MultiLocalizationPack>(jsonReader);
                }
            }
            ModLocalizationPack.Context = this;
            EventBus.Subscribe(ModLocalizationPack);
        }
        public virtual void SaveLocalization(MultiLocalizationPack localization) {
            localization.Strings.Sort((x, y) => string.Compare(x.SimpleName, y.SimpleName, true, CultureInfo.InvariantCulture));
            Directory.CreateDirectory(UserConfigFolder);
            var localizationPath = $"{LocalizationFolder}{Path.DirectorySeparatorChar}{LocalizationFile}";

            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            using (StreamWriter streamWriter = new StreamWriter(localizationPath))
            using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(jsonWriter, localization);
            }
        }
        public virtual void LoadSettings<T>(string fileName, string path, ref T setting) where T : IUpdatableSettings {
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            var assembly = ModEntry.Assembly;
            var resourcePath = $"{path}.{fileName}";
            var userPath = $"{UserConfigFolder}{Path.DirectorySeparatorChar}{fileName}";

            Directory.CreateDirectory(UserConfigFolder);
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader)) {
                setting = serializer.Deserialize<T>(jsonReader);
                setting.Init();
            }
            if (File.Exists(userPath)) {
                using (StreamReader streamReader = File.OpenText(userPath))
                using (JsonReader jsonReader = new JsonTextReader(streamReader)) {
                    try {
                        T userSettings = serializer.Deserialize<T>(jsonReader);
                        setting.OverrideSettings(userSettings);
                    } catch {
                        Logger.LogError("Failed to load user settings. Settings will be rebuilt.");
                        try { File.Copy(userPath, UserConfigFolder + $"{Path.DirectorySeparatorChar}BROKEN_{fileName}", true); } catch { Logger.LogError("Failed to archive broken settings."); }
                    }
                }
            }
            SaveSettings(fileName, setting);
        }
        public virtual void SaveSettings(string fileName, object setting) {
            Directory.CreateDirectory(UserConfigFolder);
            var userPath = $"{UserConfigFolder}{Path.DirectorySeparatorChar}{fileName}";

            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            using (StreamWriter streamWriter = new StreamWriter(userPath))
            using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(jsonWriter, setting);
            }
        }

        public virtual void BeforeBlueprintCachePatches() {
        }

        public virtual void BeforeBlueprintCacheInit() {
        }

        public virtual void AfterBlueprintCacheInit() {
        }

        public virtual void AfterBlueprintCachePatches() {
            Blueprints.GenerateUnused();
            SaveSettings(BlueprintsFile, Blueprints);
        }

        private class SortedDictonaryConverter : JsonConverter {

            private readonly StringComparer comparer;

            public SortedDictonaryConverter(StringComparer comparer) : base() {
                this.comparer = comparer;
            }

            public override bool CanConvert(Type objectType) {
                return objectType is not null
                    && objectType.IsConstructedGenericType
                    && objectType.GetGenericTypeDefinition() == typeof(SortedDictionary<,>).GetGenericTypeDefinition()
                    && objectType.GetGenericArguments()[0] == typeof(string);
            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                if (!CanConvert(objectType)) {
                    throw new Exception(string.Format("This converter is not for {0}.", objectType));
                }

                var keyType = objectType.GetGenericArguments()[0];
                var valueType = objectType.GetGenericArguments()[1];
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var sortedDictionaryType = typeof(SortedDictionary<,>).MakeGenericType(keyType, valueType);
                var tempDictonary = serializer.Deserialize(reader, dictionaryType);
                return Activator.CreateInstance(sortedDictionaryType, tempDictonary, comparer);
            }
        }
    }
}
