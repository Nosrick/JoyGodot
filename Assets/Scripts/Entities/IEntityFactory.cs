using System.Collections.Generic;
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
using JoyLib.Code.World;

namespace JoyLib.Code.Entities
{
    public interface IEntityFactory
    {
        IEntity CreateFromTemplate(IEntityTemplate template,
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
            IDriver driver = null);

        IEntity CreateLong(IEntityTemplate template,
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
            IDriver driver = null);
    }
}