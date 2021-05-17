using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Collections;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Entities.AI.Drivers;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Events;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Quests;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities
{
    [Serializable]
    public class Entity : JoyObject, IEntity
    {
        public event ValueChangedEventHandler<int> StatisticChange;
        public event ValueChangedEventHandler<int> SkillChange;
        public event ValueChangedEventHandler<int> ExperienceChange;
        public event JobChangedEventHandler JobChange;
        public event BooleanChangedEventHandler ConsciousnessChange;
        public event BooleanChangedEventHandler AliveChange;
        public event ValueChangedEventHandler<float> HappinessChange;

        protected IDictionary<string, IEntityStatistic> m_Statistics;

        protected IDictionary<string, IEntitySkill> m_Skills;

        protected IDictionary<string, INeed> m_Needs;

        protected List<IAbility> m_Abilities;

        protected EquipmentStorage m_Equipment;

        protected List<Guid> m_Backpack;

        protected IItemInstance m_NaturalWeapons;

        protected ISexuality m_Sexuality;

        protected IRomance m_Romance;

        protected List<string> m_IdentifiedItems;

        protected IJob m_CurrentJob;

        protected List<string> m_Slots;

        protected List<ICulture> m_Cultures;

        protected int m_Size;

        protected IVision m_VisionProvider;

        protected FulfillmentData m_FulfillmentData;

        protected NeedAIData m_CurrentTarget;

        public float OverallHappiness
        {
            get
            {
                if (this.HappinessIsDirty)
                {
                    float oldHappiness = this.m_CachedHappiness;
                    this.m_CachedHappiness = this.CalculateOverallHappiness();
                    this.HappinessChange?.Invoke(this, new ValueChangedEventArgs<float>
                    {
                        Delta = this.m_CachedHappiness - oldHappiness,
                        Name = "Happiness",
                        NewValue = this.m_CachedHappiness
                    });
                    this.HappinessIsDirty = false;
                }

                return this.m_CachedHappiness;
            }
        }
        protected float m_CachedHappiness;

        public bool HappinessIsDirty
        {
            get => this.m_HappinessIsDirty;
            set
            {
                this.m_HappinessIsDirty = value;
                if (this.m_HappinessIsDirty)
                {
                    float oldHappiness = this.m_CachedHappiness;
                    this.m_CachedHappiness = this.CalculateOverallHappiness();
                    this.HappinessChange?.Invoke(this, new ValueChangedEventArgs<float>
                    {
                        Delta = this.m_CachedHappiness - oldHappiness,
                        Name = "Happiness",
                        NewValue = this.m_CachedHappiness
                    });
                }
            }
        }

        protected bool m_HappinessIsDirty;


        protected NeedDataSerialisable CurrentTargetSerialise
        {
            get => new NeedDataSerialisable
            {
                idle = this.CurrentTarget.idle,
                intent = this.CurrentTarget.intent,
                need = this.CurrentTarget.need,
                searching = this.CurrentTarget.searching,
                targetGuid = this.CurrentTarget.target?.Guid ?? Guid.Empty,
                targetPoint = this.CurrentTarget.targetPoint,
                targetType = this.CurrentTarget.target is null ? "none" : this.CurrentTarget.target.GetType().Name
            };
            set
            {
                this.m_CurrentTarget = new NeedAIData
                {
                    idle = value.idle,
                    intent = value.intent,
                    need = value.need,
                    searching = value.searching,
                    targetPoint = value.targetPoint
                };

                if (value.targetType == typeof(IEntity).Name)
                {
                    this.m_CurrentTarget.target = GlobalConstants.GameManager.EntityHandler.Get(value.targetGuid);
                }
                else if (value.targetType == typeof(IItemInstance).Name)
                {
                    this.m_CurrentTarget.target = GlobalConstants.GameManager.ItemHandler.Get(value.targetGuid);
                }
            }
        }

        protected IDriver m_Driver;

        protected IPathfinder m_Pathfinder;

        protected Queue<Vector2Int> m_PathfindingData;

        [NonSerialized] protected IWorldInstance m_MyWorld;

        [NonSerialized] protected const int REGEN_TICK_TIME = 10;

        [NonSerialized] protected const int ATTACK_THRESHOLD = -50;

        public IEnumerable<IItemInstance> Contents => GlobalConstants.GameManager.ItemHandler.GetItems(this.m_Backpack);

        public IEntityRelationshipHandler RelationshipHandler { get; set; }

        public IEntitySkillHandler SkillHandler { get; set; }

        public IQuestTracker QuestTracker { get; set; }

        public NaturalWeaponHelper NaturalWeaponHelper { get; set; }

        public IDerivedValueHandler DerivedValueHandler { get; set; }

        protected readonly static string[] STANDARD_ACTIONS = new string[]
        {
            "giveitemaction",
            "fulfillneedaction",
            "seekaction",
            "wanderaction",
            "modifyrelationshippointsaction",
            "enterworldaction",
            "additemaction",
            "placeiteminworldaction"
        };

        public Entity()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="template"></param>
        /// <param name="statistics"></param>
        /// <param name="derivedValues"></param>
        /// <param name="needs"></param>
        /// <param name="skills"></param>
        /// <param name="abilities"></param>
        /// <param name="cultures"></param>
        /// <param name="job"></param>
        /// <param name="gender"></param>
        /// <param name="sex"></param>
        /// <param name="sexuality"></param>
        /// <param name="romance"></param>
        /// <param name="position"></param>
        /// <param name="sprites"></param>
        /// <param name="naturalWeapons"></param>
        /// <param name="equipment"></param>
        /// <param name="backpack"></param>
        /// <param name="identifiedItems"></param>
        /// <param name="jobs"></param>
        /// <param name="world"></param>
        /// <param name="driver"></param>
        /// <param name="roller"></param>
        /// <param name="name"></param>
        /// <param name="jobLevels"></param>
        public Entity(
            Guid guid,
            IEntityTemplate template,
            IDictionary<string, IEntityStatistic> statistics,
            IDictionary<string, IDerivedValue> derivedValues,
            IDictionary<string, INeed> needs,
            IDictionary<string, IEntitySkill> skills,
            IEnumerable<IAbility> abilities,
            IEnumerable<ICulture> cultures,
            IJob job,
            IGender gender,
            IBioSex sex,
            ISexuality sexuality,
            IRomance romance,
            Vector2Int position,
            IEnumerable<ISpriteState> sprites,
            IItemInstance naturalWeapons,
            EquipmentStorage equipment,
            IEnumerable<IItemInstance> backpack,
            IEnumerable<string> identifiedItems,
            IEnumerable<IJob> jobs,
            IWorldInstance world,
            IDriver driver,
            RNG roller = null,
            string name = null) :
            base(name,
                guid,
                derivedValues,
                position,
                STANDARD_ACTIONS,
                sprites,
                cultures.First().CultureName,
                roller,
                template.Tags.ToArray())
        {
            this.CreatureType = template.CreatureType;
            this.m_Slots = template.Slots.ToList();

            this.m_Size = template.Size;

            this.Gender = gender;

            this.Jobs = new List<IJob>(jobs);
            this.Sexuality = sexuality;
            this.Romance = romance;
            this.m_IdentifiedItems = identifiedItems.ToList();
            this.m_Statistics = statistics;

            this.m_Needs = needs;

            foreach (INeed need in this.m_Needs.Values)
            {
                need.ValueChanged -= this.MarkHappinessDirty;
                need.ValueChanged += this.MarkHappinessDirty;
            }

            this.m_Skills = skills;

            this.m_Abilities = template.Abilities.ToList();
            this.m_Abilities.AddRange(abilities);

            this.m_CurrentJob = job;

            this.Tags = template.Tags.ToList();

            this.m_NaturalWeapons = naturalWeapons;
            this.m_Equipment = equipment;
            this.m_Backpack = backpack.Select(instance => instance.Guid).ToList();
            this.Sex = sex;
            this.m_VisionProvider = template.VisionType.Copy();

            this.m_Cultures = cultures.ToList();

            this.m_Pathfinder = (IPathfinder) ScriptingEngine.Instance.FetchAndInitialise("custompathfinder");
            this.m_PathfindingData = new Queue<Vector2Int>();

            this.m_FulfillmentData = new FulfillmentData(
                "none",
                0,
                new IJoyObject[0]);

            this.RegenTicker = this.Roller.Roll(0, REGEN_TICK_TIME);

            this.MyWorld = world;
            this.JoyName = name.IsNullOrEmpty() ? this.GetNameFromMultipleCultures() : name;

            this.m_Driver = driver;
            this.PlayerControlled = driver.PlayerControlled;
            this.Data = new NonUniqueDictionary<object, object>();

            this.HappinessIsDirty = true;

            this.Initialise();

            this.CurrentTarget = NeedAIData.IdleState();
            this.ConstructDescription();

            this.StatisticChange += this.RecalculateDVs;
        }

        /// <summary>
        /// Create a new entity, naked and squirming
        /// Created with no equipment, knowledge, family, etc
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="template">The template the entity is based upon</param>
        /// <param name="statistics">The entity's statistic block</param>
        /// <param name="derivedValues">The derived values of the entity</param>
        /// <param name="needs">The entity's needs</param>
        /// <param name="skills">The entity's skill block</param>
        /// <param name="abilities">Any abilities the entity may have</param>
        /// <param name="cultures">The cultures the entity belongs to</param>
        /// <param name="job">The entity's current job</param>
        /// <param name="gender">The entity's chosen gender</param>
        /// <param name="sex">The biological sex of the entity</param>
        /// <param name="sexuality">The sexuality of the entity</param>
        /// <param name="romance">The romance type for the entity</param>
        /// <param name="position">The entity's position in its current world</param>
        /// <param name="sprites">The sprites used for the entity</param>
        /// <param name="world">The world the entity is located in</param>
        /// <param name="driver">The driver used for this entity</param>
        /// <param name="roller">The RNG used for this entity</param>
        /// <param name="name">The name of the entity</param>
        public Entity(
            Guid guid,
            IEntityTemplate template,
            IDictionary<string, IEntityStatistic> statistics,
            IDictionary<string, IDerivedValue> derivedValues,
            IDictionary<string, INeed> needs,
            IDictionary<string, IEntitySkill> skills,
            IEnumerable<IAbility> abilities,
            IEnumerable<ICulture> cultures,
            IJob job,
            IGender gender,
            IBioSex sex,
            ISexuality sexuality,
            IRomance romance,
            Vector2Int position,
            IEnumerable<ISpriteState> sprites,
            IWorldInstance world,
            IDriver driver,
            RNG roller = null,
            string name = null) :
            this(guid, template, statistics, derivedValues, needs, skills, abilities, cultures, job, gender, sex,
                sexuality, romance, position, sprites,
                null /*lobalConstants.GameManager.NaturalWeaponHelper?.MakeNaturalWeapon(template.Size)*/,
                new EquipmentStorage(template.Slots),
                new List<IItemInstance>(), new List<string>(), new List<IJob> {job}, world, driver, roller, name)
        { }

        protected void Initialise()
        {
            this.RelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;
            this.QuestTracker = GlobalConstants.GameManager.QuestTracker;
            this.SkillHandler = GlobalConstants.GameManager.SkillHandler;
            this.DerivedValueHandler = GlobalConstants.GameManager.DerivedValueHandler;
            this.NaturalWeaponHelper = GlobalConstants.GameManager.NaturalWeaponHelper;
        }

        public void Deserialise(
            IEnumerable<ICulture> cultures)
        {
            this.m_Cultures = cultures.ToList();
            this.Initialise();

            this.StatisticChange -= this.RecalculateDVs;
            this.StatisticChange += this.RecalculateDVs;
            this.HappinessIsDirty = true;
        }

        protected void MarkHappinessDirty(object sender, ValueChangedEventArgs<int> args)
        {
            this.HappinessIsDirty = true;
        }

        protected ICollection<string> ConstructDescription()
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            string relationship = null;
            if (this.PlayerControlled)
            {
                relationship = "This is You";
            }
            else
            {
                relationship = "Stranger";
                try
                {
                    relationship = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                        this.RelationshipHandler.GetBestRelationship(
                                this,
                                GlobalConstants.GameManager.Player)
                            .DisplayName);
                }
                catch (Exception e)
                { }
            }

            List<string> data = new List<string>
            {
                textInfo.ToTitleCase(this.CreatureType),
                textInfo.ToTitleCase(this.CurrentJob.Name),
                textInfo.ToTitleCase(this.Gender.Name),
                relationship,
                this.ConditionString
            };

            return data;
        }

        protected void RecalculateDVs(object sender, ValueChangedEventArgs<int> args)
        {
            if (sender != this)
            {
                return;
            }

            if (args.Delta == 0)
            {
                return;
            }

            foreach (string name in this.DerivedValues.Keys)
            {
                IDerivedValue dv = DerivedValueHandler.Calculate(name, this.Statistics.Values);
                if (this.DerivedValues[name].Base == dv.Base)
                {
                    continue;
                }

                this.DerivedValues[name].SetBase(dv.Base);
                this.OnMaximumChanged(this, new ValueChangedEventArgs<int>
                {
                    Delta = dv.Maximum - this.DerivedValues[name].Maximum,
                    Name = name,
                    NewValue = this.DerivedValues[name].Maximum
                });
            }
        }

        protected string GetNameFromMultipleCultures()
        {
            const int groupChance = 10;

            List<string> nameList = new List<string>();
            int maxNames = this.m_Cultures.SelectMany(x => x.NameData)
                .SelectMany(y => y.chain)
                .Max(z => z);

            int lastGroup = Int32.MinValue;
            for (int i = 0; i <= maxNames; i++)
            {
                ICulture random = this.m_Cultures[this.Roller.Roll(0, this.m_Cultures.Count)];

                while (random.NameData.SelectMany(x => x.chain).Max(y => y) < maxNames)
                {
                    random = this.m_Cultures[this.Roller.Roll(0, this.m_Cultures.Count)];
                }

                if (lastGroup == int.MinValue && this.Roller.Roll(0, 100) < groupChance)
                {
                    int[] groups = random.NameData.SelectMany(data => data.groups).Distinct().ToArray();

                    if (groups.Length == 0)
                    {
                        lastGroup = Int32.MinValue;
                    }
                    else
                    {
                        lastGroup = groups[this.Roller.Roll(0, groups.Length)];
                        if (random.NameData.Any(data => random.NameData.SelectMany(d => d.chain)
                                                            .Min(d => d) == i
                                                        && data.groups.Contains(lastGroup)) == false)
                        {
                            lastGroup = Int32.MinValue;
                        }
                    }
                }

                nameList.Add(random.GetNameForChain(i, this.Gender.Name, lastGroup));
            }

            this.m_Cultures.ForEach(culture => culture.ClearLastGroup());
            return String.Join(" ", nameList).Trim();
        }

        public override void Tick()
        {
            this.HappinessIsDirty = false;

            if (this.m_FulfillmentData is null == false
                && this.m_FulfillmentData.Counter > 0
                && this.m_FulfillmentData.DecrementCounter() == 0)
            {
                this.FulfillmentData = null;
            }

            this.RegenTicker += 1;
            if (this.RegenTicker == REGEN_TICK_TIME)
            {
                this.ModifyValue(DerivedValueName.HITPOINTS, 1);
                this.ModifyValue(DerivedValueName.CONCENTRATION, 1);
                this.ModifyValue(DerivedValueName.COMPOSURE, 1);
                this.ModifyValue(DerivedValueName.MANA, 1);

                this.RegenTicker = 0;

                foreach (INeed need in this.m_Needs.Values)
                {
                    this.HappinessIsDirty |= need.Tick(this);
                }
            }

            this.UpdateMe();
        }

        public void AddQuest(IQuest quest)
        {
            quest.StartQuest(this);
            QuestTracker?.AddQuest(this.Guid, quest);
        }

        public bool AddJob(IJob job)
        {
            if (this.Jobs.Any(j => j.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            this.Jobs.Add(job);
            return true;
        }

        public bool ChangeJob(string job)
        {
            if (this.Jobs.Any(j => j.Name.Equals(job, StringComparison.OrdinalIgnoreCase)))
            {
                this.m_CurrentJob = this.Jobs.First(j => j.Name.Equals(job, StringComparison.OrdinalIgnoreCase));
                this.JobChange?.Invoke(this, new JobChangedEventArgs()
                {
                    GUID = this.Guid,
                    NewJob = this.m_CurrentJob
                });
                return true;
            }

            return false;
        }

        public bool ChangeJob(IJob job)
        {
            if (this.Jobs.Any(j => j.Name.Equals(job.Name, StringComparison.OrdinalIgnoreCase)) == false)
            {
                this.Jobs.Add(job);
            }

            this.m_CurrentJob = job;
            this.JobChange?.Invoke(this, new JobChangedEventArgs()
            {
                GUID = this.Guid,
                NewJob = this.m_CurrentJob
            });

            return true;
        }

        public IEnumerable<Tuple<string, int>> GetData(IEnumerable<string> tags, params object[] args)
        {
            //Check statistics
            IEnumerable<string> tagArray = tags as string[] ?? tags.ToArray();
            List<Tuple<string, int>> data = (from tag in tagArray
                where this.m_Statistics.ContainsKey(tag)
                select new Tuple<string, int>(tag, this.m_Statistics[tag].Value)).ToList();

            //Fetch all statistics
            if (tagArray.Any(tag => tag.Equals("statistics", StringComparison.OrdinalIgnoreCase)))
            {
                data.AddRange(this.m_Statistics.Select(pair =>
                    new Tuple<string, int>(pair.Key, pair.Value.Value)));
            }

            //Check skills
            foreach (string tag in tagArray)
            {
                if (this.m_Skills.ContainsKey(tag))
                {
                    data.Add(new Tuple<string, int>(tag, this.m_Skills[tag].Value));
                }
            }

            //Fetch all skills
            if (tagArray.Any(tag => tag.Equals("skills", StringComparison.OrdinalIgnoreCase)))
            {
                data.AddRange(from IRollableValue<int> skill in this.m_Skills.Values
                    select new Tuple<string, int>(skill.Name, skill.Value));
            }

            //Check needs
            foreach (string tag in tagArray)
            {
                if (this.m_Needs.ContainsKey(tag))
                {
                    data.Add(new Tuple<string, int>(tag, this.m_Needs[tag].Value));
                }
            }

            //Fetch all needs
            if (tagArray.Any(tag => tag.Equals("needs", StringComparison.OrdinalIgnoreCase)))
            {
                data.AddRange(from INeed need in this.m_Needs select new Tuple<string, int>(need.Name, need.Value));
            }

            //Check equipment
            IEnumerable<IItemInstance> items = this.m_Equipment.Contents;

            foreach (string tag in tagArray)
            {
                int result = this.m_Equipment.Contents.Count(item => item.HasTag(tag));
                if (result > 0)
                {
                    data.Add(new Tuple<string, int>(tag, result));
                }

                result = items.Count(item => item.HasTag(tag));

                if (result > 0)
                {
                    data.Add(new Tuple<string, int>(tag, result));
                }
            }

            //Check backpack
            foreach (string tag in tagArray)
            {
                IEnumerable<IItemInstance> backpack = this.Contents;
                int identifiedNames = backpack.Count(item =>
                    item.IdentifiedName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                int unidentifiedNames = backpack.Count(item =>
                    item.ItemType.UnidentifiedName.Equals(tag, StringComparison.OrdinalIgnoreCase));

                if (identifiedNames > 0)
                {
                    data.Add(new Tuple<string, int>(tag, identifiedNames));
                }

                if (unidentifiedNames > 0)
                {
                    data.Add(new Tuple<string, int>(tag, unidentifiedNames));
                }
            }

            //Check jobs
            foreach (string tag in tagArray)
            {
                try
                {
                    IJob job = this.Jobs.FirstOrDefault(j => j.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                    if (job is null == false)
                    {
                        data.Add(new Tuple<string, int>(job.Name, 1));
                    }
                }
                catch (Exception e)
                {
                    //suppress this
                }
            }

            //Fetch all job levels
            if (tagArray.Any(tag => tag.Equals("jobs", StringComparison.OrdinalIgnoreCase)))
            {
                data.AddRange(from job in this.Jobs select new Tuple<string, int>(job.Name, 1));
            }

            //Fetch gender data
            if (tagArray.Any(tag => tag.Equals(this.Gender.Name, StringComparison.OrdinalIgnoreCase))
                || tagArray.Any(tag => tag.Equals("gender", StringComparison.OrdinalIgnoreCase)))
            {
                data.Add(new Tuple<string, int>(this.Gender.Name, 1));
            }

            //Fetch sex data
            if (tagArray.Any(tag => tag.Equals(this.Sex.Name, StringComparison.OrdinalIgnoreCase))
                || tagArray.Any(tag => tag.Equals("sex")))
            {
                data.Add(new Tuple<string, int>(this.Sex.Name, 1));
            }

            if (tagArray.Any(tag => tag.Equals("can birth", StringComparison.OrdinalIgnoreCase)))
            {
                data.Add(new Tuple<string, int>("can birth", this.Sex.CanBirth == true ? 1 : 0));
            }

            //Fetch sexuality data
            if (tagArray.Any(tag => tag.Equals(this.Sexuality.Name, StringComparison.OrdinalIgnoreCase))
                || tagArray.Any(tag => tag.Equals("sexuality", StringComparison.OrdinalIgnoreCase)))
            {
                data.Add(new Tuple<string, int>(this.Sexuality.Name, 1));
            }

            //Fetch romance data
            if (tagArray.Any(tag => tag.Equals(this.Romance.Name, StringComparison.OrdinalIgnoreCase))
                || tagArray.Any(tag => tag.Equals("romance", StringComparison.OrdinalIgnoreCase)))
            {
                data.Add(new Tuple<string, int>(this.Romance.Name, 1));
            }

            if (args is null || args.Length <= 0)
            {
                return data.ToArray();
            }

            foreach (object obj in args)
            {
                if (!(obj is Entity other))
                {
                    continue;
                }

                List<IRelationship> relationships = RelationshipHandler?.GetAllForObject(this).ToList();

                if (relationships.IsNullOrEmpty())
                {
                    return data.ToArray();
                }

                if (tagArray.Any(tag => tag.Equals("will mate", StringComparison.OrdinalIgnoreCase)))
                {
                    data.Add(new Tuple<string, int>(
                        other.JoyName,
                        this.Sexuality.WillMateWith(this, other, relationships) ? 1 : 0));
                }

                if (tagArray.Any(tag => tag.Equals("will romance", StringComparison.OrdinalIgnoreCase)))
                {
                    data.Add(new Tuple<string, int>(
                        other.JoyName,
                        this.Romance.WillRomance(this, other, relationships) ? 1 : 0));
                }

                //Check relationships
                foreach (IRelationship relationship in relationships)
                {
                    foreach (string tag in tagArray)
                    {
                        if (relationship.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                        {
                            int relationshipValue = relationship.GetRelationshipValue(this.Guid, other.Guid);
                            data.Add(new Tuple<string, int>(tag, 1));
                            data.Add(new Tuple<string, int>("relationship", relationshipValue));
                        }
                    }
                }
            }

            return data.ToArray();
        }

        public void UpdateMe()
        {
            this.HasMoved = false;

            if (this.Conscious == false)
            {
                return;
            }

            this.VisionProvider.Update(this, this.MyWorld);
            this.m_Driver.Locomotion(this);
        }

        public void SetPath(Queue<Vector2Int> pointsRef)
        {
            this.m_PathfindingData = pointsRef;
        }

        public void AddIdentifiedItem(string nameRef)
        {
            this.m_IdentifiedItems.Add(nameRef);
        }

        public new void Move(Vector2Int position)
        {
            base.Move(position);
            foreach (IJoyObject joyObject in this.Contents)
            {
                joyObject.Move(position);
            }
        }

        public virtual bool RemoveContents(IItemInstance item)
        {
            if (this.m_Backpack.Contains(item.Guid))
            {
                this.m_Backpack.Remove(item.Guid);
                this.ItemRemoved?.Invoke(this, new ItemChangedEventArgs() {Item = item});
                return true;
            }

            return false;
        }

        public virtual bool RemoveItemFromPerson(IItemInstance item)
        {
            //Check slots first
            bool result = this.Equipment.RemoveContents(item);

            //Then the backpack
            result |= this.RemoveContents(item);
            return result;
        }

        public virtual bool RemoveEquipment(IItemInstance item)
        {
            return this.Equipment.RemoveContents(item);
        }

        public IItemInstance[] SearchBackpackForItemType(IEnumerable<string> tags)
        {
            try
            {
                List<IItemInstance> matchingItems = new List<IItemInstance>();
                foreach (IItemInstance item in this.Contents)
                {
                    int matches = 0;
                    foreach (string tag in tags)
                    {
                        if (item.HasTag(tag))
                        {
                            matches++;
                        }
                    }

                    if (matches > 0)
                    {
                        matchingItems.Add(item);
                    }
                }

                return matchingItems.ToArray();
            }
            catch (Exception ex)
            {
                GlobalConstants.ActionLog.Log("ERROR WHEN SEARCHING BACKPACK OF " + this);
                GlobalConstants.ActionLog.StackTrace(ex);
                return new List<IItemInstance>().ToArray();
            }
        }

        public virtual bool EquipItem(IItemInstance itemRef)
        {
            if (!this.Equipment.CanAddContents(itemRef))
            {
                return false;
            }

            return this.Equipment.AddContents(itemRef);
        }

        public virtual bool UnequipItem(IItemInstance actor)
        {
            return this.Equipment.Contains(actor) && this.Equipment.RemoveContents(actor);
        }

        public void AddExperience(int value)
        {
            int result = this.CurrentJob.AddExperience(value);
            this.ExperienceChange?.Invoke(this, new ValueChangedEventArgs<int>
            {
                Delta = value,
                Name = "experience",
                NewValue = result
            });
        }

        public void DamageMe(int value, Entity source)
        {
            int damage = value;

            this.DamageValue(DerivedValueName.HITPOINTS, damage);
        }

        public IItemInstance GetEquipment(string slotRef)
        {
            return this.Equipment.GetSlotContents(slotRef);
        }

        public virtual bool AddContents(IItemInstance actor)
        {
            if (this.m_IdentifiedItems.Any(i => i.Equals(actor.JoyName, StringComparison.OrdinalIgnoreCase)))
            {
                actor.IdentifyMe();
            }

            actor.MyWorld = this.MyWorld;
            actor.Move(this.WorldPosition);

            if (actor is ItemInstance goItem)
            {
                /*
                if (goItem.MonoBehaviourHandler is null)
                {
                    goItem.Instantiate();
                }

                goItem.MonoBehaviourHandler.gameObject.SetActive(false);
                */
            }

            if (this.m_Backpack.Contains(actor.Guid) == false)
            {
                this.m_Backpack.Add(actor.Guid);
            }

            this.ItemAdded?.Invoke(this, new ItemChangedEventArgs {Item = actor});
            return true;
        }

        public virtual bool CanAddContents(IItemInstance actor)
        {
            return true;
        }

        public virtual bool Contains(IItemInstance actor)
        {
            bool result = false;
            result |= this.m_Backpack.Contains(actor.Guid);
            if (result)
            {
                return true;
            }

            foreach (IItemInstance item in this.Contents)
            {
                result |= item.Contains(actor);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool AddContents(IEnumerable<IItemInstance> actors)
        {
            foreach (IItemInstance actor in actors)
            {
                if (this.m_IdentifiedItems.Any(i => i.Equals(actor.JoyName, StringComparison.OrdinalIgnoreCase)))
                {
                    actor.IdentifyMe();
                }
            }

            this.m_Backpack.AddRange(
                actors.Where(actor => this.m_Backpack.Any(item => item == actor.Guid) == false)
                    .Select(instance => instance.Guid));
            foreach (IItemInstance actor in actors)
            {
                this.ItemAdded?.Invoke(this, new ItemChangedEventArgs() {Item = actor});
            }

            return true;
        }

        public virtual bool RemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.RemoveContents(actor));
        }

        public virtual void Clear()
        {
            this.m_Backpack.Clear();
        }

        public override int GetValue(string name)
        {
            if (this.Statistics.ContainsKey(name))
            {
                return this.Statistics[name].Value;
            }

            return this.Skills.ContainsKey(name) ? this.Skills[name].Value : base.GetValue(name);
        }

        public override int ModifyValue(string name, int value)
        {
            if (this.Statistics.ContainsKey(name))
            {
                this.Statistics[name].ModifyValue(value);
                this.StatisticChange?.Invoke(this, new ValueChangedEventArgs<int>
                {
                    Delta = value,
                    Name = name,
                    NewValue = this.Statistics[name].Value
                });
                return this.Statistics[name].Value;
            }

            if (this.Skills.ContainsKey(name))
            {
                this.Skills[name].ModifyValue(value);
                this.SkillChange?.Invoke(this, new ValueChangedEventArgs<int>
                {
                    Delta = value,
                    Name = name,
                    NewValue = this.Skills[name].Value
                });
                return this.Skills[name].Value;
            }

            bool lastConscious = this.Conscious;
            bool lastAlive = this.Alive;
            int result = base.ModifyValue(name, value);
            if (this.Conscious != lastConscious)
            {
                this.ConsciousnessChange?.Invoke(this, new BooleanChangeEventArgs {Value = this.Conscious});
            }

            if (this.Alive != lastAlive)
            {
                this.AliveChange?.Invoke(this, new BooleanChangeEventArgs {Value = this.Alive});
            }

            return result;
        }

        protected float CalculateOverallHappiness()
        {
            return this.Needs.Values.Average(need => need.PercentageFull);
        }

        public override int SetValue(string name, int value)
        {
            if (!this.Statistics.ContainsKey(name) && !this.Skills.ContainsKey(name))
            {
                bool lastConscious = this.Conscious;
                bool lastAlive = this.Alive;
                int result = base.SetValue(name, value);
                if (this.Conscious != lastConscious)
                {
                    this.ConsciousnessChange?.Invoke(this, new BooleanChangeEventArgs {Value = this.Conscious});
                }

                if (this.Alive != lastAlive)
                {
                    this.AliveChange?.Invoke(this, new BooleanChangeEventArgs {Value = this.Alive});
                }

                return result;
            }

            if (this.Statistics.ContainsKey(name))
            {
                int old = this.Statistics[name].Value;
                this.Statistics[name].SetValue(value);
                this.StatisticChange?.Invoke(this, new ValueChangedEventArgs<int>
                {
                    Delta = value - old,
                    Name = name,
                    NewValue = this.Statistics[name].Value
                });
                return this.Statistics[name].Value;
            }

            if (this.Skills.ContainsKey(name))
            {
                int old = this.Skills[name].Value;
                this.Skills[name].SetValue(value);
                this.SkillChange?.Invoke(this, new ValueChangedEventArgs<int>
                {
                    Delta = value - old,
                    Name = name,
                    NewValue = this.Skills[name].Value
                });
                return this.Skills[name].Value;
            }

            throw new InvalidOperationException("No value of " + name + " found on " + this.JoyName);
        }

        public string ContentString { get; }
        public event ItemRemovedEventHandler ItemRemoved;
        public event ItemAddedEventHandler ItemAdded;

        public string CreatureType { get; protected set; }

        public IBioSex Sex { get; protected set; }

        public IGender Gender { get; protected set; }

        public NeedAIData CurrentTarget
        {
            get { return this.m_CurrentTarget; }
            set { this.m_CurrentTarget = value; }
        }

        public IDriver Driver => this.m_Driver;

        public List<IAbility> AllAbilities
        {
            get
            {
                List<IAbility> allAbilities = this.Abilities;
                allAbilities.AddRange(this.Equipment.Contents.SelectMany(item => item.AllAbilities));
                return allAbilities;
            }
        }

        public EquipmentStorage Equipment => this.m_Equipment;

        public IDictionary<string, IEntityStatistic> Statistics
        {
            get { return this.m_Statistics; }
        }

        public IDictionary<string, IEntitySkill> Skills
        {
            get { return this.m_Skills; }
        }

        public IDictionary<string, INeed> Needs
        {
            get { return this.m_Needs; }
        }

        public List<IAbility> Abilities => this.m_Abilities;

        public string JobName
        {
            get { return this.m_CurrentJob.Name; }
        }

        public bool Sentient
        {
            get { return this.Tags.Any(tag => tag.Equals("sentient", StringComparison.OrdinalIgnoreCase)); }
        }

        public int Size
        {
            get { return this.m_Size; }
        }

        public IEnumerable<Vector2Int> Vision
        {
            get { return this.m_VisionProvider.Vision; }
        }

        public bool PlayerControlled { get; set; }

        public IItemInstance NaturalWeapons => this.m_NaturalWeapons;

        public List<string> IdentifiedItems => this.m_IdentifiedItems;

        public IJob CurrentJob => this.m_CurrentJob;

        public bool HasMoved { get; set; }

        public FulfillmentData FulfillmentData
        {
            get => this.m_FulfillmentData;
            set
            {
                this.m_FulfillmentData = value;

                if (value is null)
                {
                    this.MyNode.SetSpeechBubble(false);
                    return;
                }

                if (this.m_FulfillmentData.Name.IsNullOrEmpty() == false
                    && this.m_FulfillmentData.Name.Equals("none", StringComparison.OrdinalIgnoreCase) == false)
                {
                    this.MyNode.SetSpeechBubble(this.m_FulfillmentData.Counter > 0,
                        this.m_Needs[this.m_FulfillmentData.Name].FulfillingSprite);
                }
            }
        }

        public ISexuality Sexuality
        {
            get => this.m_Sexuality;
            set => this.m_Sexuality = value;
        }

        public IRomance Romance
        {
            get => this.m_Romance;
            set => this.m_Romance = value;
        }

        public IAbility TargetingAbility { get; set; }

        public int Mana
        {
            get { return this.DerivedValues[DerivedValueName.MANA].Maximum; }
        }

        public int ManaRemaining
        {
            get { return this.DerivedValues[DerivedValueName.MANA].Value; }
        }

        public int ComposureRemaining
        {
            get { return this.DerivedValues[DerivedValueName.COMPOSURE].Value; }
        }

        public int Composure
        {
            get { return this.DerivedValues[DerivedValueName.COMPOSURE].Maximum; }
        }

        public int Concentration
        {
            get { return this.DerivedValues[DerivedValueName.CONCENTRATION].Maximum; }
        }

        public int ConcentrationRemaining
        {
            get { return this.DerivedValues[DerivedValueName.CONCENTRATION].Value; }
        }

        public Vector2Int TargetPoint { get; set; }

        protected int RegenTicker { get; set; }

        //public Quest QuestOffered { get; set; }

        public List<ICulture> Cultures => this.m_Cultures;

        public IVision VisionProvider => this.m_VisionProvider;

        public List<string> Slots => this.m_Slots;

        public int VisionMod => 7;

        protected string ConditionString
        {
            get
            {
                float percentage = this.HitPointsRemaining / (float) this.HitPoints;
                if (this.LastPercentage == percentage && this.LastConditionString.IsNullOrEmpty() == false)
                {
                    return this.LastConditionString;
                }

                /*
                string condition = this.Equals(GlobalConstants.GameManager.Player)
                    ? "You are "
                    : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Gender.PersonalSubject)
                      + " " + this.Gender.IsOrAre + " ";
                      */
                string condition = "This is fine";
                /*
                if (this.Conscious == false)
                {
                    condition += "unconscious";
                }
                else if (percentage > 0.9f)
                {
                    condition += "uninjured";
                }
                else if (percentage > 0.7f)
                {
                    condition += "lightly injured";
                }
                else if (percentage > 0.5f)
                {
                    condition += "moderately injured";
                }
                else if (percentage > 0.3f)
                {
                    condition += "severely injured";
                }
                else if (percentage > 0.1f)
                {
                    condition += "gravely injured";
                }
                else
                {
                    condition += "barely conscious";
                }
                */

                this.LastPercentage = percentage;
                this.LastConditionString = condition;
                return condition;
            }
        }

        protected float LastPercentage { get; set; }
        protected string LastConditionString { get; set; }

        public Queue<Vector2Int> PathfindingData
        {
            get { return this.m_PathfindingData; }
            set { this.m_PathfindingData = value; }
        }

        public IPathfinder Pathfinder
        {
            get { return this.m_Pathfinder; }
        }

        public override IWorldInstance MyWorld
        {
            get => this.m_MyWorld;
            set
            {
                this.m_MyWorld = value;
                foreach (IItemInstance item in this.Contents)
                {
                    item.MyWorld = value;
                }
            }
        }

        public bool Conscious => this.HitPointsRemaining > 0;

        public List<IJob> Jobs { get; protected set; }

        public override ICollection<string> Tooltip => this.ConstructDescription();


        public List<string> CultureNames
        {
            get
            {
                if (this.m_CultureNames is null)
                {
                    this.m_CultureNames = this.Cultures.Select(culture => culture.CultureName).ToList();
                }

                return this.m_CultureNames;
            }
            protected set => this.m_CultureNames = value;
        }

        protected List<string> m_CultureNames;
    }
}