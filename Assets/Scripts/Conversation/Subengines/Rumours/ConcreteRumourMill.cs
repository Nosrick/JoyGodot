using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours
{
    public class ConcreteRumourMill : IRumourMill
    {
        public List<IRumour> Rumours { get; protected set; }

        public List<IRumour> RumourTypes { get; protected set; }

        public JSONValueExtractor ValueExtractor { get; protected set; }

        public RNG Roller { get; protected set; }

        public ConcreteRumourMill(RNG roller = null)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Roller = roller is null ? new RNG() : roller;
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.RumourTypes is null)
            {
                this.Rumours = new List<IRumour>();
                this.RumourTypes = this.LoadRumours();
            }
        }

        protected List<IRumour> LoadRumours()
        {
            List<IRumour> rumours = new List<IRumour>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Rumours",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.",
                        LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> rumourCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Rumours");

                foreach (Dictionary rumour in rumourCollection)
                {
                    string text = this.ValueExtractor.GetValueFromDictionary<string>(rumour, "Text");
                    string processor = rumour.Contains("Processor")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(rumour, "Processor")
                        : "";

                    float viralPotential = rumour.Contains("ViralPotential")
                        ? this.ValueExtractor.GetValueFromDictionary<float>(rumour, "ViralPotential")
                        : 1f;

                    IEnumerable<string> tags = rumour.Contains("Tags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(rumour, "Tags")
                        : new string[0];

                    IEnumerable<string> conditionStrings = rumour.Contains("Conditions")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(rumour, "Conditions")
                        : new string[0];

                    IEnumerable<string> parameters = rumour.Contains("Parameters")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(rumour, "Parameters")
                        : new string[0];

                    bool baseless = !rumour.Contains("Baseless")
                                    || this.ValueExtractor.GetValueFromDictionary<bool>(rumour, "Baseless");
                    float lifetimeMultiplier = rumour.Contains("LifetimeMultiplier")
                        ? this.ValueExtractor.GetValueFromDictionary<float>(rumour, "LifetimeMultiplier")
                        : 1f;
                    int lifetime = rumour.Contains("Lifetime")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(rumour, "Lifetime")
                        : BaseRumour.DEFAULT_LIFETIME;

                    List<ITopicCondition> conditions =
                        conditionStrings.Select(this.ParseCondition).ToList();

                    var processorBase = processor.IsNullOrEmpty() ? null : GlobalConstants.ScriptingEngine.FetchAndInitialise(processor);
                    if (processorBase is null == false)
                    {
                        IRumour processorObject = (IRumour) processorBase;
                        rumours.Add(
                            processorObject.Create(
                                null,
                                tags,
                                viralPotential,
                                conditions.ToArray(),
                                parameters,
                                text,
                                lifetimeMultiplier,
                                lifetime,
                                baseless));
                    }
                    else
                    {
                        rumours.Add(
                            new BaseRumour(
                                null,
                                tags,
                                viralPotential,
                                conditions.ToArray(),
                                parameters,
                                text,
                                lifetimeMultiplier,
                                lifetime,
                                baseless));
                    }
                }
            }

            return rumours;
        }

        public IRumour GetRandom(IWorldInstance overworldRef)
        {
            if (this.Rumours.Count == 0)
            {
                IJoyObject left = overworldRef.GetRandomSentientWorldWide();
                IJoyObject right = left.MyWorld.GetRandomSentient();
                this.Rumours.Add(this.GenerateRandomRumour(new[] {left, right}));
            }

            return this.Rumours[this.Roller.Roll(0, this.Rumours.Count)];
        }

        public IRumour GenerateRandomRumour(IJoyObject[] participants)
        {
            if (this.RumourTypes is null)
            {
                this.Initialise();
            }

            IRumour rumour = null;
            IRumour[] possibilities = this.RumourTypes.Where(r => r.FulfilsConditions(participants)).ToArray();

            if (possibilities.Length > 0)
            {
                IRumour selected = possibilities[this.Roller.Roll(0, possibilities.Length)];
                rumour = selected.Create(
                    participants,
                    selected.Tags,
                    selected.ViralPotential,
                    selected.Conditions,
                    selected.Parameters,
                    selected.Words,
                    selected.LifetimeMultiplier,
                    selected.Lifetime);
            }
            else
            {
                IRumour selected = this.RumourTypes[this.Roller.Roll(0, this.RumourTypes.Count)];
                rumour = selected.Create(
                    participants,
                    selected.Tags,
                    selected.ViralPotential,
                    selected.Conditions,
                    selected.Parameters,
                    selected.Words,
                    selected.LifetimeMultiplier,
                    selected.Lifetime,
                    true);
            }

            return rumour;
        }

        public IRumour GenerateRumourFromTags(IJoyObject[] participants, string[] tags)
        {
            if (this.RumourTypes is null)
            {
                this.Initialise();
            }

            IRumour rumour = null;

            IRumour[] possibilities = this.RumourTypes.Where(r =>
                    r.Tags.Intersect(tags, StringComparer.OrdinalIgnoreCase).Any() && r.FulfilsConditions(participants))
                .ToArray();

            if (possibilities.Length > 0)
            {
                IRumour resultingRumour = possibilities[this.Roller.Roll(0, possibilities.Length)];
                rumour = resultingRumour.Create(
                    participants,
                    resultingRumour.Tags,
                    resultingRumour.ViralPotential,
                    resultingRumour.Conditions,
                    resultingRumour.Parameters,
                    resultingRumour.Words,
                    resultingRumour.LifetimeMultiplier,
                    resultingRumour.Lifetime);
            }
            else
            {
                int result = this.Roller.Roll(0, this.RumourTypes.Count);
                IRumour resultingRumour = this.RumourTypes[result];
                rumour = resultingRumour.Create(
                    participants,
                    resultingRumour.Tags,
                    resultingRumour.ViralPotential,
                    resultingRumour.Conditions,
                    resultingRumour.Parameters,
                    resultingRumour.Words,
                    resultingRumour.LifetimeMultiplier,
                    resultingRumour.Lifetime,
                    true);
            }

            return rumour;
        }

        public IRumour[] GenerateOneRumourOfEachType(IJoyObject[] participants)
        {
            List<IRumour> rumours = new List<IRumour>();
            foreach (IRumour type in this.RumourTypes)
            {
                rumours.Add(type.Create(
                    participants,
                    type.Tags,
                    type.ViralPotential,
                    type.Conditions,
                    type.Parameters,
                    type.Words,
                    type.LifetimeMultiplier,
                    type.Lifetime,
                    type.Baseless));
            }

            return rumours.ToArray();
        }

        protected ITopicCondition ParseCondition(string conditionString)
        {
            try
            {
                string[] split = conditionString.Split(new char[] {'<', '>', '=', '!'},
                    StringSplitOptions.RemoveEmptyEntries);

                string criteria = split[0].Trim();
                string operand = conditionString.First(c => c.Equals('!')
                                                            || c.Equals('=')
                                                            || c.Equals('<')
                                                            || c.Equals('>')).ToString();
                string stringValue = split[1].Trim();

                TopicConditionFactory factory = new TopicConditionFactory();

                int value = criteria.Equals("relationship", StringComparison.OrdinalIgnoreCase) && operand.Equals("=")
                    ? 1
                    : int.Parse(stringValue);

                criteria = criteria.Equals("relationship", StringComparison.OrdinalIgnoreCase) && operand.Equals("=")
                    ? stringValue
                    : criteria;

                return factory.Create(criteria, operand, value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not parse conversation condition line " + conditionString);
            }
        }

        public void Dispose()
        {
            this.RumourTypes = null;
            this.Rumours = null;
        }
    }
}