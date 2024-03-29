﻿using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Abilities;

namespace JoyGodot.Assets.Scripts.Entities.Jobs
{
    public interface IJob
    {
        int GetSkillDiscount(string skillName);

        int GetStatisticDiscount(string statisticName);

        int AddExperience(int value);
        bool SpendExperience(int value);

        IJob Copy(IJob original);

        IDictionary<IAbility, int> Abilities { get; }
        
        string Name { get; }
        
        string Description { get; }
        
        int Experience { get; }
        
        IDictionary<string, int> StatisticDiscounts { get; }
        
        IDictionary<string, int> SkillDiscounts { get; }
        
        Color AbilityIconColour { get; }
        Color AbilityBackgroundColour { get; }
    }
}