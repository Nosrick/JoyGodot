using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Scripts.Cultures
{
    public interface ICulture
    {
        string GetRandomName(string genderRef);

        string GetNameForChain(int chain, string gender, int group = Int32.MinValue);

        IBioSex ChooseSex(IEnumerable<IBioSex> sexes);

        ISexuality ChooseSexuality(IEnumerable<ISexuality> sexualities);

        IRomance ChooseRomance(IEnumerable<IRomance> romances);

        IGender ChooseGender(string sex, IEnumerable<IGender> genders);

        IJob ChooseJob(IEnumerable<IJob> jobs);

        IDictionary<string, IEntityStatistic> GetStats(IDictionary<string, IEntityStatistic> baseStats);

        void ClearLastGroup();

        int GetStatVariance(string statistic);
        
        string Tileset { get; }
        
        int LastGroup { get; }
        
        string[] Inhabitants { get; }
        
        string CultureName { get; }
        
        string[] RulerTypes { get; }
        
        string[] Crimes { get; }
        
        string[] RelationshipTypes { get; }
        
        string[] RomanceTypes { get; }
        
        string[] Sexes { get; }
        
        string[] Sexualities { get; }
        
        string[] Genders { get; }
        
        string[] Jobs { get; }
        
        int NonConformingGenderChance { get; }
        
        NameData[] NameData { get; }
        
        RNG Roller { get; }
        
        IDictionary<string, IDictionary<string, Color>> CursorColours { get; }
        IDictionary<string, IDictionary<string, Color>> BackgroundColours { get; }
        
        IDictionary<string, Color> FontColours { get; }
    }
}