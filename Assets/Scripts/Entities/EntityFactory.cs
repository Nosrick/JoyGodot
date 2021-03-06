using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.Drivers;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities
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
                selectedGender = dominantCulture.ChooseGender(selectedSex.Name, this.GenderHandler.Values);
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
                    .GetManagedSprites(
                        dominantCulture.Tileset, 
                        template.CreatureType, 
                        "idle")
                    .ToList();
                var firstIndices = spriteData.First().RandomiseIndices();

                for (int i = 0; i < spriteData.Count; i++)
                {
                    SpriteData data = spriteData[i];
                    data.SetColourIndices(firstIndices);
                    states.Add(new SpriteState(
                        data.Name,
                        dominantCulture.Tileset,
                        data));
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