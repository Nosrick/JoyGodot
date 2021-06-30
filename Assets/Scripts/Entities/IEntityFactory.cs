using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.Drivers;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities
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