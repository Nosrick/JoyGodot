using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.Drivers;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Graphics;
using JoyLib.Code.Managers;
using JoyLib.Code.Physics;
using JoyLib.Code.Rollers;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities
{
    public class EntityFactory : IEntityFactory
    {
        protected INeedHandler NeedHandler { get; set; }

        protected IObjectIconHandler ObjectIcons { get; set; }

        protected ICultureHandler CultureHandler { get; set; }

        protected IEntitySexualityHandler SexualityHandler { get; set; }

        protected IEntityBioSexHandler BioSexHandler { get; set; }

        protected IGenderHandler GenderHandler { get; set; }

        protected IEntityRomanceHandler RomanceHandler { get; set; }

        protected IJobHandler JobHandler { get; set; }

        protected IPhysicsManager PhysicsManager { get; set; }

        protected IEntitySkillHandler SkillHandler { get; set; }

        protected IDerivedValueHandler DerivedValueHandler { get; set; }

        protected GUIDManager GuidManager { get; set; }

        protected RNG Roller { get; set; }

        public EntityFactory(
            GUIDManager guidManager,
            INeedHandler needHandler,
            IObjectIconHandler objectIconHandler,
            ICultureHandler cultureHandler,
            IEntitySexualityHandler sexualityHandler,
            IEntityBioSexHandler sexHandler,
            IGenderHandler genderHandler,
            IEntityRomanceHandler romanceHandler,
            IJobHandler jobHandler,
            IPhysicsManager physicsManager,
            IEntitySkillHandler skillHandler,
            IDerivedValueHandler derivedValueHandler,
            RNG roller)
        {
            this.GuidManager = guidManager;
            this.Roller = roller;
            this.NeedHandler = needHandler;
            this.ObjectIcons = objectIconHandler;
            this.CultureHandler = cultureHandler;
            this.SexualityHandler = sexualityHandler;
            this.BioSexHandler = sexHandler;
            this.JobHandler = jobHandler;
            this.RomanceHandler = romanceHandler;
            this.GenderHandler = genderHandler;
            this.PhysicsManager = physicsManager;
            this.SkillHandler = skillHandler;
            this.DerivedValueHandler = derivedValueHandler;
        }

        public IEntity CreateFromTemplate(IEntityTemplate template,
            Vector2Int position,
            string name = null,
            IDictionary<string, IEntityStatistic> statistics = null,
            IDictionary<string, IDerivedValue> derivedValues = null,
            IDictionary<string, IEntitySkill> skills = null,
            IEnumerable<IAbility> abilities = null,
            IEnumerable<ICulture> cultures = null,
            IGender gender = null,
            IBioSex sex = null,
            ISexuality sexuality = null,
            IRomance romance = null,
            IJob job = null,
            IEnumerable<ISpriteState> sprites = null,
            IWorldInstance world = null,
            IDriver driver = null)
        {
            string selectedName = name;
            IJob selectedJob = job;
            IGender selectedGender = gender;
            IBioSex selectedSex = sex;
            ISexuality selectedSexuality = sexuality;
            IRomance selectedRomance = romance;
            IEnumerable<ISpriteState> selectedSprites = sprites;
            List<ICulture> creatureCultures = new List<ICulture>();
            IDriver selectedDriver = driver;
            IDictionary<string, IEntityStatistic> selectedStatistics = statistics;
            IDictionary<string, IDerivedValue> selectedDVs = derivedValues;
            IDictionary<string, IEntitySkill> selectedSkills = skills;
            IEnumerable<IAbility> selectedAbilities = abilities;
            if (!(cultures is null))
            {
                creatureCultures.AddRange(cultures);
            }
            else
            {
                creatureCultures = new List<ICulture>();
                List<ICulture> cultureTypes = this.CultureHandler.GetByCreatureType(template.CreatureType);
                creatureCultures.AddRange(cultureTypes);
            }

            IDictionary<string, INeed> needs = new Dictionary<string, INeed>();

            foreach (string need in template.Needs)
            {
                needs.Add(need, this.NeedHandler.GetRandomised(need));
            }

            int result = this.Roller.Roll(0, creatureCultures.Count);
            ICulture dominantCulture = creatureCultures[result];

            if (selectedStatistics is null)
            {
                selectedStatistics = dominantCulture.GetStats(template.Statistics);
            }

            if (selectedDVs is null)
            {
                selectedDVs = new Dictionary<string, IDerivedValue>(
                    this.DerivedValueHandler.GetEntityStandardBlock(
                        selectedStatistics.Values));
            }

            if (selectedSkills is null)
            {
                selectedSkills = this.SkillHandler.GetDefaultSkillBlock();
                foreach (EntitySkill skill in template.Skills.Values)
                {
                    selectedSkills.Add(skill.Name, skill);
                }
            }

            if (selectedAbilities is null)
            {
                selectedAbilities = new List<IAbility>();
            }

            if (selectedJob is null)
            {
                selectedJob = dominantCulture.ChooseJob(this.JobHandler.Values);
            }

            if (selectedSex is null)
            {
                selectedSex = dominantCulture.ChooseSex(this.BioSexHandler.Values);
            }

            if (selectedGender is null)
            {
                selectedGender = dominantCulture.ChooseGender(selectedSex, this.GenderHandler.Values);
            }

            if (selectedName.IsNullOrEmpty())
            {
                selectedName = dominantCulture.GetRandomName(selectedGender.Name);
            }

            if (selectedRomance is null)
            {
                selectedRomance = dominantCulture.ChooseRomance(this.RomanceHandler.Values);
            }

            if (selectedSexuality is null)
            {
                selectedSexuality = dominantCulture.ChooseSexuality(this.SexualityHandler.Values);
            }

            if (selectedSprites is null)
            {
                List<ISpriteState> states = new List<ISpriteState>();
                List<SpriteData> spriteData = this.ObjectIcons
                    .GetSprites(dominantCulture.CultureName, template.CreatureType, "idle").ToList();
                for (int i = 0; i < spriteData.Count; i++)
                {
                    SpriteData data = spriteData[i];
                    states.Add(new SpriteState(
                        data.m_Name,
                        data));
                }

                states[0].RandomiseColours();
                for (int i = 1; i < states.Count; i++)
                {
                    states[i].SetColourIndices(states[0].GetIndices());
                }

                selectedSprites = states;
            }

            if (selectedDriver is null)
            {
                selectedDriver = new StandardDriver();
            }

            IEntity entity = new Entity(
                this.GuidManager.AssignGUID(),
                template,
                selectedStatistics,
                selectedDVs,
                needs,
                selectedSkills,
                selectedAbilities,
                creatureCultures,
                selectedJob,
                selectedGender,
                selectedSex,
                selectedSexuality,
                selectedRomance,
                position,
                selectedSprites,
                world,
                selectedDriver,
                new RNG(),
                selectedName);

            return entity;
        }

        public IEntity CreateLong(IEntityTemplate template,
            IDictionary<string, INeed> needs,
            IDictionary<string, IEntityStatistic> statistics,
            IDictionary<string, IDerivedValue> derivedValues,
            IDictionary<string, IEntitySkill> skills,
            IEnumerable<IAbility> abilities,
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
            IEnumerable<ICulture> cultures = null,
            IDriver driver = null)
        {
            List<ICulture> creatureCultures = new List<ICulture>();
            if (cultures != null)
            {
                creatureCultures.AddRange(cultures);
            }
            else
            {
                List<ICulture> cultureTypes = this.CultureHandler.GetByCreatureType(template.CreatureType);
                creatureCultures.AddRange(cultureTypes);
            }

            IDriver selectedDriver = driver;
            if (selectedDriver is null)
            {
                selectedDriver = new StandardDriver();
            }

            IEntity entity = new Entity(
                this.GuidManager.AssignGUID(),
                template,
                statistics,
                derivedValues,
                needs,
                skills,
                abilities,
                creatureCultures,
                job,
                gender,
                sex,
                sexuality,
                romance,
                position,
                sprites,
                naturalWeapons,
                equipment,
                backpack,
                identifiedItems,
                jobs,
                world,
                selectedDriver,
                new RNG());

            return entity;
        }
    }
}