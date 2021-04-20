using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
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

        [Signal]
        public delegate void ValueChanged(string name, int newIndex);

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
                    .ToArray());
            
            item = this.AddItem(
                "Culture",
                this.CultureHandler.GetByCreatureType(item.Value)
                    .Select(c => c.CultureName)
                    .ToArray());

            ICulture culture = this.CultureHandler.GetByCultureName(item.Value);

            if (culture is null)
            {
                return;
            }

            item = this.AddItem(
                "Gender",
                culture.Genders);

            item = this.AddItem(
                "Sex",
                culture.Sexes);

            item = this.AddItem(
                "Romance",
                culture.RomanceTypes);

            item = this.AddItem(
                "Sexuality",
                culture.Sexualities);
        }

        protected StringValueItem AddItem(string name, ICollection<string> values)
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
            if (!item.IsConnected(
                "ValueChanged",
                this,
                "OnValueChange"))
            {
                item.Connect(
                    "ValueChanged",
                    this,
                    "OnValueChange");
            }

            return item;
        }

        public void OnValueChange(string name, int delta, int newIndex)
        {
            GD.Print(nameof(this.OnValueChange));
            GD.Print(name + " : " + delta + " : " + newIndex);
        }
    }
}
