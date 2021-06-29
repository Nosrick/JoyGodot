using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.GUI.Tools;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.CharacterCreationState
{
    public class BasicPlayerInfo : Control
    {
        protected PackedScene ListItemPrefab { get; set; }

        protected List<StringValueItem> Parts { get; set; }

        public IEntityTemplateHandler EntityTemplateHandler { get; set; }
        public ICultureHandler CultureHandler { get; set; }
        public IGenderHandler GenderHandler { get; set; }
        public IEntityBioSexHandler BioSexHandler { get; set; }
        public IEntitySexualityHandler SexualityHandler { get; set; }
        public IEntityRomanceHandler RomanceHandler { get; set; }
        
        public IJobHandler JobHandler { get; set; }

        public ICulture CurrentCulture { get; protected set; }
        public IEntityTemplate CurrentTemplate { get; protected set; }
        public string CurrentGender { get; protected set; }
        
        public string CurrentSex { get; protected set; }
        
        public string CurrentRomance { get; protected set; }
        
        public string CurrentSexuality { get; protected set; }
        
        public string CurrentJob { get; protected set; }

        [Signal]
        public delegate void ValueChanged(string name, string newValue);

        public override void _Ready()
        {
            this.Parts = new List<StringValueItem>();
            this.ListItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/String List Item.tscn");

            IGameManager gameManager = GlobalConstants.GameManager;
            this.EntityTemplateHandler = gameManager.EntityTemplateHandler;
            this.CultureHandler = gameManager.CultureHandler;
            this.GenderHandler = gameManager.GenderHandler;
            this.BioSexHandler = gameManager.BioSexHandler;
            this.SexualityHandler = gameManager.SexualityHandler;
            this.RomanceHandler = gameManager.RomanceHandler;
            this.JobHandler = gameManager.JobHandler;

            this.SetUp();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            foreach (var item in this.Parts)
            {
                if (item.IsConnected(
                    "ValueChanged",
                    this,
                    "OnValueChange"))
                {
                    item.Disconnect(
                        "ValueChanged",
                        this,
                        "OnValueChange");
                }
            }
        }

        protected void SetUp()
        {
            foreach (var obj in this.Parts)
            {
                obj.Visible = false;
            }

            var item = this.AddItem(
                "Species",
                this.EntityTemplateHandler.Values
                    .Select(template => template.CreatureType)
                    .ToArray(),
                nameof(this.OnTemplateChange));

            this.CurrentTemplate = this.EntityTemplateHandler.Get(item.Value);
            item.Tooltip = new List<string>
            {
                "What species you belong to."
            };

            item = this.AddItem(
                "Culture",
                this.CultureHandler.GetByCreatureType(item.Value)
                    .Select(c => c.CultureName)
                    .ToArray(),
                nameof(this.OnCultureChange));

            this.CurrentCulture = this.CultureHandler.GetByCultureName(item.Value);
            item.Tooltip = new List<string>
            {
                "The culture you hail from."
            };

            if (this.CurrentCulture is null)
            {
                return;
            }

            item = this.AddItem(
                "Job",
                this.CurrentCulture.Jobs,
                nameof(this.OnValueChange));

            this.CurrentJob = item.Value;
            item.Tooltip = new List<string>
            {
               GlobalConstants.GameManager.JobHandler.Get(this.CurrentJob).Description
            };

            item = this.AddItem(
                "Sex",
                this.CurrentCulture.Sexes,
                nameof(this.OnValueChange));

            this.CurrentSex = item.Value;
            item.Tooltip = new List<string>
            {
                "Your biological sex."
            };

            item = this.AddItem(
                "Gender",
                this.CurrentCulture.Genders,
                nameof(this.OnValueChange));

            this.CurrentGender = item.Value;
            item.Tooltip = new List<string>
            {
                "Your gender presentation."
            };

            item = this.AddItem(
                "Romance",
                this.CurrentCulture.RomanceTypes,
                nameof(this.OnValueChange));

            this.CurrentRomance = item.Value;
            item.Tooltip = new List<string>
            {
                "Your romantic preference."
            };

            item = this.AddItem(
                "Sexuality",
                this.CurrentCulture.Sexualities,
                nameof(this.OnValueChange));

            this.CurrentSexuality = item.Value;
            item.Tooltip = new List<string>
            {
                "Your sexual preference."
            };
        }

        protected void OnChange(bool randomCulture = false)
        {
            var tempPart = this.GetItem("species");

            if (tempPart is null)
            {
                GD.PushError("Could not find Species selector!");
                return;
            }

            this.CurrentTemplate = this.EntityTemplateHandler.Get(tempPart.Value);

            tempPart = this.GetItem("culture");

            if (tempPart is null)
            {
                GD.PushError("Could not find Culture selector!");
                return;
            }
            
            if (randomCulture)
            {
                tempPart.Values = this.CultureHandler.GetByCreatureType(this.CurrentTemplate.CreatureType)
                    .Select(culture => culture.CultureName)
                    .ToArray();
                tempPart.Value = tempPart.Values.GetRandom();
            }
            this.CurrentCulture = this.CultureHandler.GetByCultureName(tempPart.Value);

            tempPart = this.GetItem("job");

            this.CurrentJob = tempPart.Value;
            tempPart.Tooltip = new List<string>
            {
                GlobalConstants.GameManager.JobHandler.Get(this.CurrentJob).Description
            };

            var bioSexPart = this.GetItem("sex");

            if (bioSexPart is null)
            {
                GD.PushError("Could not find Sex selector!");
                return;
            }

            bioSexPart.Values = this.CurrentCulture.Sexes;
            bioSexPart.Value = this.CurrentCulture.ChooseSex(this.BioSexHandler.Values).Name;

            var genderPart = this.GetItem("gender");

            if (genderPart is null)
            {
                GD.PushError("Could not find Gender selector!");
                return;
            }

            genderPart.Values = this.CurrentCulture.Genders;
            genderPart.Value =
                this.CurrentCulture.ChooseGender(bioSexPart.Value, this.GenderHandler.Values).Name;
            this.CurrentGender = genderPart.Value;

            tempPart = this.GetItem("romance");

            if (tempPart is null)
            {
                GD.PushError("Could not find Romance selector!");
                return;
            }

            tempPart.Values = this.CurrentCulture.RomanceTypes;
            tempPart.Value = this.CurrentCulture.ChooseRomance(this.RomanceHandler.Values).Name;

            tempPart = this.GetItem("sexuality");

            if (tempPart is null)
            {
                GD.PushError("Could not find Sexuality selector!");
                return;
            }

            tempPart.Values = this.CurrentCulture.Sexualities;
            tempPart.Value = this.CurrentCulture.ChooseSexuality(this.SexualityHandler.Values).Name;
        }

        protected StringValueItem AddItem(
            string name,
            ICollection<string> values,
            string connection)
        {
            var inactive = this.Parts.FirstOrDefault(part => part.Visible == false);
            StringValueItem item;
            if (inactive is null == false)
            {
                item = inactive;
            }
            else
            {
                if (!(this.ListItemPrefab.Instance() is StringValueItem stringValueItem))
                {
                    GD.PushError(nameof(this.ListItemPrefab) + " was not of correct type, or was not loaded!");
                    return null;
                }

                item = stringValueItem;
            }

            this.AddChild(item);
            this.Parts.Add(item);
            item.ValueName = name;
            item.Values = values;
            foreach (var obj in item.GetSignalConnectionList("ValueChanged"))
            {
                item.Disconnect("ValueChanged", this, obj.ToString());
            }

            if (!item.IsConnected(
                "ValueChanged",
                this,
                connection))
            {
                item.Connect(
                    "ValueChanged",
                    this,
                    connection);
            }

            return item;
        }

        protected void UpdateValues()
        {
            var tempPart = this.GetItem("species");
            if (tempPart is null)
            {
                GD.PushError("Could not find Species selector!");
                return;
            }
            this.CurrentTemplate = this.EntityTemplateHandler.Get(tempPart.Value);

            tempPart = this.GetItem("culture");
            if (tempPart is null)
            {
                GD.PushError("Could not find Culture selector!");
                return;
            }
            this.CurrentCulture = this.CultureHandler.GetByCultureName(tempPart.Value);

            tempPart = this.GetItem("job");
            this.CurrentJob = tempPart.Value;

            var bioSexPart = this.GetItem("sex");
            if (bioSexPart is null)
            {
                GD.PushError("Could not find Sex selector!");
                return;
            }
            this.CurrentSex = bioSexPart.Value;

            var genderPart = this.GetItem("gender");
            if (genderPart is null)
            {
                GD.PushError("Could not find Gender selector!");
                return;
            }
            this.CurrentGender = genderPart.Value;

            tempPart = this.GetItem("romance");
            if (tempPart is null)
            {
                GD.PushError("Could not find Romance selector!");
                return;
            }
            this.CurrentRomance = tempPart.Value;

            tempPart = this.GetItem("sexuality");
            if (tempPart is null)
            {
                GD.PushError("Could not find Sexuality selector!");
                return;
            }
            this.CurrentSexuality = tempPart.Value;
        }

        protected StringValueItem GetItem(string name)
        {
            return this.Parts.FirstOrDefault(part => part.ValueName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void OnValueChange(string name, int delta, string newValue)
        {
            this.UpdateValues();
            this.EmitSignal("ValueChanged", name, newValue);
        }

        public void OnCultureChange(string name, int delta, string newValue)
        {
            this.OnChange();
            this.EmitSignal("ValueChanged", "Culture", newValue);
        }

        public void OnTemplateChange(string name, int delta, string newValue)
        {
            this.OnChange(true);
            this.EmitSignal("ValueChanged", "Template", newValue);
        }
    }
}