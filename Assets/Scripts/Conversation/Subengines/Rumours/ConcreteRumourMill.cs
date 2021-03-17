using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Conversation.Conversations.Rumours;
using JoyLib.Code.Conversation.Subengines.Rumours;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JoyLib.Code.Conversation.Conversations
{
    public class ConcreteRumourMill : IRumourMill
    {
        public List<IRumour> Rumours { get; protected set; }

        public List<IRumour> RumourTypes { get; protected set; }

        public RNG Roller { get; protected set; }

        public ConcreteRumourMill(RNG roller = null)
        {
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
                Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Rumours",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
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

                            foreach (JToken child in jToken["Rumours"])
                            {
                                string text = (string) child["Text"];
                                string processor = (string) child["Processor"] ?? "NONE";
                                float viralPotential = (float) (child["ViralPotential"] ?? 1.0f);
                                IEnumerable<string> tags = child["Tags"] is null
                                    ? new string[0]
                                    : child["Tags"].Select(token => (string) token);

                                IEnumerable<string> conditionStrings = child["Conditions"] is null
                                    ? new string[0]
                                    : child["Conditions"].Select(token => (string) token);

                                IEnumerable<string> parameters = child["Parameters"] is null
                                    ? new string[0]
                                    : child["Parameters"].Select(token => (string) token);

                                bool baseless = (bool) (child["Baseless"] ?? true);
                                float lifetimeMultiplier = (float) (child["LifetimeMultiplier"] ?? 1.0f);
                                int lifetime = (int) (child["Lifetime"] ?? BaseRumour.DEFAULT_LIFETIME);

                                List<ITopicCondition> conditions =
                                    conditionStrings.Select(this.ParseCondition).ToList();

                                if (processor.Equals("NONE", StringComparison.OrdinalIgnoreCase))
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
                                else
                                {
                                    IRumour processorObject = (IRumour) ScriptingEngine.Instance.FetchAndInitialise(processor);
                                    if (processorObject is null)
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
                                    else
                                    {
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
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load rumours from file " + file,
                                LogLevel.Error);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
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
                this.Rumours.Add(this.GenerateRandomRumour(new []{left, right}));
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
                string[] split = conditionString.Split(new char[] {'<', '>', '=', '!'}, StringSplitOptions.RemoveEmptyEntries);

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

                criteria = criteria.Equals("relationship", StringComparison.OrdinalIgnoreCase) && operand.Equals("=") ? stringValue : criteria;

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