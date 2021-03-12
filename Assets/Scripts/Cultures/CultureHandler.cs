using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Cultures
{
    public class CultureHandler : ICultureHandler
    {
        protected System.Collections.Generic.Dictionary<string, ICulture> m_Cultures;

        public IEnumerable<ICulture> Values => this.m_Cultures.Values;
        
        protected IObjectIconHandler IconHandler { get; set; }

        public CultureHandler(IObjectIconHandler objectIconHandler)
        {
            this.IconHandler = objectIconHandler;
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.m_Cultures is null)
            {
                this.m_Cultures = this.Load().ToDictionary(x => x.CultureName, x => x);
            }
        }

        public ICulture Get(string name)
        {
            return this.GetByCultureName(name);
        }

        public bool Add(ICulture value)
        {
            if (this.m_Cultures.ContainsKey(value.CultureName))
            {
                return false;
            }
            this.m_Cultures.Add(value.CultureName, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (!this.m_Cultures.ContainsKey(key))
            {
                return false;
            }
            this.m_Cultures[key] = null;
            this.m_Cultures.Remove(key);
            return true;

        }

        public IEnumerable<ICulture> Load()
        {
            string folderPath = Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Cultures";
            string[] files = Directory.GetFiles(folderPath, "*.json");

            JSONValueExtractor jsonValueExtractor = new JSONValueExtractor();

            //IObjectIconHandler objectIcons = GlobalConstants.GameManager.ObjectIconHandler;

            List<ICulture> cultures = new List<ICulture>();

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON in " + file, LogLevel.Warning);
                    continue;
                }

                if (!(result.Result is Dictionary cultureDict))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                Array cultureArray = (Array) cultureDict["Cultures"];

                foreach (Dictionary culture in cultureArray)
                {
                    string cultureName = jsonValueExtractor.GetValueFromDictionary<string>(culture, "CultureName");
                    GlobalConstants.ActionLog.Log("Name: " + cultureName);

                    int nonconformingGenderChance =
                        jsonValueExtractor.GetValueFromDictionary<int>(culture, "NonConformingGenderChance");
                    GlobalConstants.ActionLog.Log("Trans Chance: " + nonconformingGenderChance);

                    float number = 0;
                    ICollection<string> rulers = jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Rulers");
                    GlobalConstants.ActionLog.Log("Rulers");
                    GlobalConstants.ActionLog.Log(rulers);

                    Array entry = new Array();
                    ICollection<string> crimes = jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Crimes");
                    GlobalConstants.ActionLog.Log("Crimes");
                    GlobalConstants.ActionLog.Log(crimes);

                    ICollection<string> inhabitants =
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Inhabitants");
                    GlobalConstants.ActionLog.Log("Inhabitants");
                    GlobalConstants.ActionLog.Log(inhabitants);
                    
                    ICollection<string> relationships = 
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Relationships");
                    GlobalConstants.ActionLog.Log("Relationships");
                    GlobalConstants.ActionLog.Log(relationships);

                    List<NameData> nameDatas = new List<NameData>();
                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Names");
                    ICollection<Dictionary> nameData = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    foreach (var data in nameData)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(data, "Name");

                        ICollection<int> chain = jsonValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Chain");

                        ICollection<int> groups = jsonValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Group");

                        ICollection<string> genders = jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Gender");

                        nameDatas.Add(new NameData(
                            name,
                            chain.ToArray(),
                            genders.ToArray(),
                            groups.ToArray()));
                    }
                    
                    GlobalConstants.ActionLog.Log(nameData);

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Sexualities");
                    ICollection<Dictionary> inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> sexualities = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        sexualities.Add(name, weight);
                    }

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Sexes");
                    inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> sexes = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        sexes.Add(name, weight);
                    }

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Romances");
                    inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> romances = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        romances.Add(name, weight);
                    }

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Genders");
                    inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> genderChances = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        genderChances.Add(name, weight);
                    }

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Jobs");
                    inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> jobs = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        jobs.Add(name, weight);
                    }

                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Statistics");
                    inner = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, Tuple<int, int>> statistics =
                        new System.Collections.Generic.Dictionary<string, Tuple<int, int>>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        int magnitude = jsonValueExtractor.GetValueFromDictionary<int>(dict, "Magnitude");
                        statistics.Add(
                            name,
                            new Tuple<int, int>(
                                weight, magnitude));
                    }

                    this.IconHandler.AddSpriteDataFromJson(culture);

                    IDictionary<string, IDictionary<string, Color>> backgroundColours =
                        this.ExtractColourData(culture, "BackgroundColours");
                }

                //string cultureName = 
            }
            /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken["Cultures"])
                            {
                                string cultureName = (string) child["CultureName"];
                                int nonConformingGenderChance = (int) child["NonConformingGenderChance"];
                                IEnumerable<string> rulers = child["Rulers"].Select(token => (string) token);
                                IEnumerable<string> crimes = child["Crimes"].Select(token => (string) token);
                                IEnumerable<string> inhabitants =
                                    child["Inhabitants"].Select(token => (string) token);
                                IEnumerable<string> relationships =
                                    child["Relationships"].Select(token => (string) token);

                                JToken dataArray = child["Names"];
                                List<NameData> nameData = new List<NameData>();
                                foreach (var data in dataArray)
                                {
                                    string name = (string) data["Name"];
                                    int[] chain = data["Chain"]?.Select(token => (int) token).ToArray();
                                    if (chain.IsNullOrEmpty())
                                    {
                                        chain = new[] {0};
                                    }

                                    string[] genderNames = data["Gender"]?.Select(token => (string) token).ToArray();
                                    if (genderNames.IsNullOrEmpty())
                                    {
                                        genderNames = new[] {"all"};
                                    }

                                    int[] groups = data["Group"]?.Select(token => (int) token).ToArray();
                                    if (groups.IsNullOrEmpty())
                                    {
                                        groups = new int[0];
                                    }
                                    nameData.Add(new NameData(
                                        name,
                                        chain,
                                        genderNames,
                                        groups));
                                }

                                dataArray = child["Sexualities"];
                                IDictionary<string, int> sexualities = dataArray.Select(token =>
                                        new KeyValuePair<string, int>(
                                            (string) token["Name"],
                                            (int) token["Chance"]))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["Romances"];
                                IDictionary<string, int> romances = dataArray.Select(token =>
                                        new KeyValuePair<string, int>(
                                            (string) token["Name"],
                                            (int) token["Chance"]))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["Genders"];
                                IDictionary<string, int> genders = dataArray.Select(token =>
                                        new KeyValuePair<string, int>(
                                            (string) token["Name"],
                                            (int) token["Chance"]))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["Sexes"];
                                IDictionary<string, int> sexes = dataArray.Select(token =>
                                        new KeyValuePair<string, int>(
                                            (string) token["Name"],
                                            (int) token["Chance"]))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["Statistics"];
                                IDictionary<string, Tuple<int, int>> statistics = dataArray.Select(token =>
                                        new KeyValuePair<string, Tuple<int, int>>(
                                            (string) token["Name"],
                                            new Tuple<int, int>(
                                                (int) token["Chance"],
                                                (int) token["Magnitude"])))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["Jobs"];
                                IDictionary<string, int> jobPrevalence = dataArray.Select(token =>
                                        new KeyValuePair<string, int>(
                                            (string) token["Name"],
                                            (int) token["Chance"]))
                                    .ToDictionary(x => x.Key, x => x.Value);

                                dataArray = child["TileSet"];
                                string tileSetName = (string) dataArray["Name"];

                                objectIcons.AddSpriteDataFromJson(tileSetName, dataArray["SpriteData"]);

                                dataArray = child["UIColours"];

                                IDictionary<string, IDictionary<string, Color>> cursorColours =
                                    new Dictionary<string, IDictionary<string, Color>>();
                                try
                                {
                                    cursorColours = this.ExtractColourData(dataArray, "CursorColours");
                                }
                                catch (Exception e)
                                {
                                    GlobalConstants.ActionLog.AddText(
                                        "Could not find cursor colours in file " + file,
                                        LogLevel.Error);
                                    GlobalConstants.ActionLog.StackTrace(e);
                                    cursorColours.Add(
                                        "DefaultCursor",
                                        new Dictionary<string, Color>
                                        {
                                            {"default", Color.magenta}
                                        });
                                }

                                IDictionary<string, IDictionary<string, Color>> backgroundColours =
                                    new Dictionary<string, IDictionary<string, Color>>();
                                try
                                {
                                    backgroundColours = this.ExtractColourData(dataArray, "BackgroundColours");
                                }
                                catch (Exception e)
                                {
                                    GlobalConstants.ActionLog.AddText(
                                        "Could not find background colours in file " + file,
                                        LogLevel.Warning);
                                    GlobalConstants.ActionLog.StackTrace(e);
                                    backgroundColours.Add(
                                        "DefaultWindow",
                                        new Dictionary<string, Color>
                                        {
                                            {"default", Color.magenta}
                                        });
                                }

                                IDictionary<string, Color> mainFontColours = new Dictionary<string, Color>();
                                try
                                {
                                    var fontColours = dataArray["FontColours"];
                                    foreach (var colour in fontColours)
                                    {
                                        mainFontColours.Add(
                                            (string) colour["Name"],
                                            GraphicsHelper.ParseHTMLString((string) colour["Value"]));
                                    }
                                }
                                catch (Exception e)
                                {
                                    GlobalConstants.ActionLog.AddText(
                                        "Could not find main font colour in file " + file,
                                        LogLevel.Warning);
                                    GlobalConstants.ActionLog.StackTrace(e);
                                    mainFontColours.Add(
                                        "Font",
                                        Color.black);
                                }

                                cultures.Add(
                                    new CultureType(
                                        cultureName,
                                        tileSetName,
                                        rulers,
                                        crimes,
                                        nameData,
                                        jobPrevalence,
                                        inhabitants,
                                        sexualities,
                                        sexes,
                                        statistics,
                                        relationships,
                                        romances,
                                        genders,
                                        nonConformingGenderChance,
                                        backgroundColours,
                                        cursorColours,
                                        mainFontColours));
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load cultures from file: " + file,
                                LogLevel.Error);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                        finally
                        {
                            jsonReader.Close();
                            reader.Close();
                        }
                    }
                }
            }
            */

            return cultures;
        }

        protected IDictionary<string, IDictionary<string, Color>> ExtractColourData(
            Dictionary element,
            string elementName)
        {
            IDictionary<string, IDictionary<string, Color>> colours =
                new System.Collections.Generic.Dictionary<string, IDictionary<string, Color>>();

            if (element.Contains(elementName))
            {
                
            }
            /*
            foreach(var colour in element)
            {
                string name = (string) colour["Name"];
                foreach (var data in colour["Colour"])
                {
                    string partName = (string) data["Name"];
                    Color c = new Color((string) data["Value"]);

                    if (colours.ContainsKey(name))
                    {
                        colours[name].Add(partName, c);
                    }
                    else
                    {
                        colours.Add(name, new Dictionary<string, Color>
                        {
                            {partName, c}
                        });
                    }
                }
            }
            */
            return colours;
        }

        public ICulture GetByCultureName(string name)
        {
            this.Initialise();

            if (this.m_Cultures.ContainsKey(name))
            {
                return this.m_Cultures[name];
            }

            return null;
        }

        public List<ICulture> GetByCreatureType(string type)
        {
            this.Initialise();

            try
            {
                System.Collections.Generic.Dictionary<string, ICulture> cultures = this.m_Cultures.Where(
                        culture => culture.Value.Inhabitants.Contains(
                            type, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                return cultures.Values.ToList();
            }
            catch (Exception e)
            {
                GD.PrintErr("Could not find a culture for creature type " + type);
                throw new InvalidOperationException("Could not find a culture for creature type " + type, e);
            }
        }

        public IEnumerable<ICulture> Cultures
        {
            get
            {
                this.Initialise();

                return this.m_Cultures.Values.ToArray();
            }
        }

        public void Dispose()
        {
            string[] keys = this.m_Cultures.Keys.ToArray();
            foreach (string key in keys)
            {
                this.m_Cultures[key] = null;
            }

            this.m_Cultures = null;
        }
    }
}