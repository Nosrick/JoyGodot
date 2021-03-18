using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public JSONValueExtractor ValueExtractor { get; protected set; }

        protected IObjectIconHandler IconHandler { get; set; }

        public CultureHandler(IObjectIconHandler objectIconHandler)
        {
            this.IconHandler = objectIconHandler;
            this.ValueExtractor = new JSONValueExtractor();
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
            string folderPath = Directory.GetCurrentDirectory() + 
                                GlobalConstants.ASSETS_FOLDER + 
                                GlobalConstants.DATA_FOLDER + 
                                "Cultures";
            string[] files = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);

            List<ICulture> cultures = new List<ICulture>();

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary cultureDict))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                Array cultureArray = this.ValueExtractor.GetValueFromDictionary<Array>(cultureDict, "Cultures");

                foreach (Dictionary culture in cultureArray)
                {
                    string cultureName = this.ValueExtractor.GetValueFromDictionary<string>(culture, "CultureName");

                    int nonconformingGenderChance =
                        this.ValueExtractor.GetValueFromDictionary<int>(culture, "NonConformingGenderChance");

                    ICollection<string> rulers =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Rulers");

                    ICollection<string> crimes =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Crimes");

                    ICollection<string> inhabitants =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Inhabitants");

                    ICollection<string> relationships =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(culture, "Relationships");

                    List<NameData> nameDatas = new List<NameData>();
                    Array entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Names");
                    ICollection<Dictionary> nameData = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    foreach (var data in nameData)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(data, "Name");

                        ICollection<int> chain =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Chain");

                        ICollection<int> groups =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<int>(data, "Group");

                        ICollection<string> genders =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Gender");

                        nameDatas.Add(new NameData(
                            name,
                            chain.ToArray(),
                            genders.ToArray(),
                            groups.ToArray()));
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Sexualities");
                    ICollection<Dictionary> inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> sexualities = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        sexualities.Add(name, weight);
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Sexes");
                    inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> sexes = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        sexes.Add(name, weight);
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Romances");
                    inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> romances = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        romances.Add(name, weight);
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Genders");
                    inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> genderChances = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        genderChances.Add(name, weight);
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Jobs");
                    inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, int> jobs = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        jobs.Add(name, weight);
                    }

                    entry = this.ValueExtractor.GetValueFromDictionary<Array>(culture, "Statistics");
                    inner = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry);
                    IDictionary<string, Tuple<int, int>> statistics =
                        new System.Collections.Generic.Dictionary<string, Tuple<int, int>>();
                    foreach (Dictionary dict in inner)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                        int weight = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Chance");
                        int magnitude = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Magnitude");
                        statistics.Add(
                            name,
                            new Tuple<int, int>(
                                weight, magnitude));
                    }

                    this.IconHandler.AddSpriteDataFromJson(culture);

                    string tileSetName = this.ValueExtractor.GetValueFromDictionary<string>(
                        this.ValueExtractor.GetValueFromDictionary<Dictionary>(
                            culture, 
                            "TileSet"),
                        "Name");

                    Dictionary uiColours = this.ValueExtractor.GetValueFromDictionary<Dictionary>(culture, "UIColours");

                    IDictionary<string, IDictionary<string, Color>> backgroundColours =
                        this.ExtractColourData(
                            uiColours,
                            "BackgroundColours");

                    IDictionary<string, IDictionary<string, Color>> cursorColours =
                        this.ExtractColourData(
                            uiColours,
                            "CursorColours");

                    IDictionary<string, Color> fontColours =
                        this.ExtractFontData(
                            uiColours,
                            "FontColours");
                    
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
            string elementName)
        {
            IDictionary<string, IDictionary<string, Color>> colours =
                new System.Collections.Generic.Dictionary<string, IDictionary<string, Color>>();

            if (element.Contains(elementName))
            {
                ICollection<Dictionary> elementsData = this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                    this.ValueExtractor.GetValueFromDictionary<Array>(
                        element,
                        elementName));
                foreach (Dictionary inner in elementsData)
                {
                    string partName = this.ValueExtractor.GetValueFromDictionary<string>(inner, "Name");
                    ICollection<Dictionary> innerDicts = this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                        this.ValueExtractor.GetValueFromDictionary<Array>(inner, "Colour"));
                    foreach (Dictionary colourData in innerDicts)
                    {
                        string colourName = this.ValueExtractor.GetValueFromDictionary<string>(colourData, "Name");
                        Color colour = new Color(this.ValueExtractor.GetValueFromDictionary<string>(colourData, "Value"));

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
            string elementName)
        {
            IDictionary<string, Color> colours = new System.Collections.Generic.Dictionary<string, Color>();

            if (element.Contains(elementName))
            {
                ICollection<Dictionary> elementsData = this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                    this.ValueExtractor.GetValueFromDictionary<Array>(
                        element,
                        elementName));

                foreach (Dictionary inner in elementsData)
                {
                    string partName = this.ValueExtractor.GetValueFromDictionary<string>(inner, "Name");
                    Color colour = new Color(this.ValueExtractor.GetValueFromDictionary<string>(inner, "Value"));

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