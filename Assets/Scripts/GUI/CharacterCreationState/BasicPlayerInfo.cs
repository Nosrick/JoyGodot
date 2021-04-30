using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Helpers;
using JoyLib.Code.Unity.GUI;

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
                "OnTemplateChange");

            this.CurrentTemplate = this.EntityTemplateHandler.Get(item.Value);

            item = this.AddItem(
                "Culture",
                this.CultureHandler.GetByCreatureType(item.Value)
                    .Select(c => c.CultureName)
                    .ToArray(),
                "OnCultureChange");

            this.CurrentCulture = this.CultureHandler.GetByCultureName(item.Value);

            if (this.CurrentCulture is null)
            {
                return;
            }

            item = this.AddItem(
                "Job",
                this.CurrentCulture.Jobs,
                "OnJobChange");

            this.CurrentJob = item.Value;

            item = this.AddItem(
                "Sex",
                this.CurrentCulture.Sexes,
                "OnValueChange");

            this.CurrentSex = item.Value;

            item = this.AddItem(
                "Gender",
                this.CurrentCulture.Genders,
                "OnValueChange");

            this.CurrentGender = item.Value;

            item = this.AddItem(
                "Romance",
                this.CurrentCulture.RomanceTypes,
                "OnValueChange");

            this.CurrentRomance = item.Value;

            item = this.AddItem(
                "Sexuality",
                this.CurrentCulture.Sexualities,
                "OnValueChange");

            this.CurrentSexuality = item.Value;
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

        protected StringValueItem GetItem(string name)
        {
            return this.Parts.FirstOrDefault(part => part.ValueName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void OnValueChange(string name, int delta, int newIndex)
        {
            GD.Print(nameof(this.OnValueChange));
            GD.Print(name + " : " + delta + " : " + newIndex);
        }

        public void OnCultureChange(string name, int delta, int newIndex)
        {
            this.OnChange();
            this.EmitSignal("ValueChanged", "Culture", this.CurrentCulture.CultureName);
        }

        public void OnTemplateChange(string name, int delta, int newIndex)
        {
            this.OnChange(true);
            this.EmitSignal("ValueChanged", "Template", this.CurrentTemplate.CreatureType);
        }

        public void OnJobChange(string name, int delta, int newIndex)
        {
            this.CurrentJob = this.GetItem("job").Value;
        }
    }
}