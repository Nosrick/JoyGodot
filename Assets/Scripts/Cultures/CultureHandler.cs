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

                    int nonconformingGenderChance =
                        jsonValueExtractor.GetValueFromDictionary<int>(culture, "NonConformingGenderChance");

                    float number = 0;
                    ICollection<string> rulers =
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Rulers");

                    Array entry = new Array();
                    ICollection<string> crimes =
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Crimes");

                    ICollection<string> inhabitants =
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Inhabitants");

                    ICollection<string> relationships =
                        jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Relationships");

                    List<NameData> nameDatas = new List<NameData>();
                    entry = jsonValueExtractor.GetValueFromDictionary<Array>(culture, "Names");
                    ICollection<Dictionary> nameData = jsonValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    foreach (var data in nameData)
                    {
                        string name = jsonValueExtractor.GetValueFromDictionary<string>(data, "Name");

                        ICollection<int> chain =
                            jsonValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Chain");

                        ICollection<int> groups =
                            jsonValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Group");

                        ICollection<string> genders =
                            jsonValueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Gender");

                        nameDatas.Add(new NameData(
                            name,
                            chain.ToArray(),
                            genders.ToArray(),
                            groups.ToArray()));
                    }

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

                    string tileSetName = jsonValueExtractor.GetValueFromDictionary<string>(
                        jsonValueExtractor.GetValueFromDictionary<Dictionary>(
                            culture, 
                            "TileSet"),
                        "Name");

                    Dictionary uiColours = jsonValueExtractor.GetValueFromDictionary<Dictionary>(culture, "UIColours");

                    IDictionary<string, IDictionary<string, Color>> backgroundColours =
                        this.ExtractColourData(
                            uiColours,
                            "BackgroundColours",
                            jsonValueExtractor);

                    IDictionary<string, IDictionary<string, Color>> cursorColours =
                        this.ExtractColourData(
                            uiColours,
                            "CursorColours",
                            jsonValueExtractor);

                    IDictionary<string, Color> fontColours =
                        this.ExtractFontData(
                            uiColours,
                            "FontColours",
                            jsonValueExtractor);
                    
                    cultures.Add(
                        new CultureType(
                            cultureName,
                            tileSetName,
                            rulers,
                            crimes,
                            nameDatas,
                            jobs,
                            inhabitants,
                            sexualities,
                            sexes,
                            statistics,
                            relationships,
                            romances,
                            genderChances,
                            nonconformingGenderChance,
                            backgroundColours,
                            cursorColours,
                            fontColours));
                }
            }

            return cultures;
        }

        protected IDictionary<string, IDictionary<string, Color>> ExtractColourData(
            Dictionary element,
            string elementName,
            JSONValueExtractor valueExtractor)
        {
            IDictionary<string, IDictionary<string, Color>> colours =
                new System.Collections.Generic.Dictionary<string, IDictionary<string, Color>>();

            if (element.Contains(elementName))
            {
                ICollection<Dictionary> elementsData = valueExtractor.GetCollectionFromArray<Dictionary>(
                    valueExtractor.GetValueFromDictionary<Array>(
                        element,
                        elementName));
                foreach (Dictionary inner in elementsData)
                {
                    string partName = valueExtractor.GetValueFromDictionary<string>(inner, "Name");
                    ICollection<Dictionary> innerDicts = valueExtractor.GetCollectionFromArray<Dictionary>(
                        valueExtractor.GetValueFromDictionary<Array>(inner, "Colour"));
                    foreach (Dictionary colourData in innerDicts)
                    {
                        string colourName = valueExtractor.GetValueFromDictionary<string>(colourData, "Name");
                        Color colour = new Color(valueExtractor.GetValueFromDictionary<string>(colourData, "Value"));

                        if (colours.ContainsKey(partName))
                        {
                            colours[partName].Add(colourName, colour);
                        }
                        else
                        {
                            colours.Add(
                                partName,
                                new System.Collections.Generic.Dictionary<string, Color>
                                {
                                    {colourName, colour}
                                });
                        }
                    }
                }
            }

            return colours;
        }

        protected IDictionary<string, Color> ExtractFontData(
            Dictionary element,
            string elementName,
            JSONValueExtractor valueExtractor)
        {
            IDictionary<string, Color> colours = new System.Collections.Generic.Dictionary<string, Color>();

            if (element.Contains(elementName))
            {
                ICollection<Dictionary> elementsData = valueExtractor.GetCollectionFromArray<Dictionary>(
                    valueExtractor.GetValueFromDictionary<Array>(
                        element,
                        elementName));

                foreach (Dictionary inner in elementsData)
                {
                    string partName = valueExtractor.GetValueFromDictionary<string>(inner, "Name");
                    Color colour = new Color(valueExtractor.GetValueFromDictionary<string>(inner, "Value"));

                    colours.Add(partName, colour);
                }
            }

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