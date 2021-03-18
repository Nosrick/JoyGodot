from JoyLib.Code.Helpers import TemporaryWeaponMaker
from Abilities.Abstracts import AbstractAbility
from JoyLib.Code.Entities.Abilities import AbilityTrigger
from JoyLib.Code.Entities.Abilities import AbilityTarget
from JoyLib.Code.Combat import CombatEngine
from JoyLib.Code.Helpers import ActionLog

class ChaosBolt(AbstractAbility.AbstractAbility):
    def __init__(self):
        self.abilityTrigger = AbilityTrigger.OnUse
        self.target = AbilityTarget.Ranged
        self.stacking = False
        self.name = "Chaos Bolt"
        self.internalName = "ChaosBolt"
        self.description = "A bolt of chaotic energy. Could be devastating, could be weedy."
        self.file = __file__
        self.counter = 1
        self.magnitude = 1
        self.priority = 1
        self.manaCost = 3

    def Use(self, user, target, ability):
        tempWeapon = TemporaryWeaponMaker.Make(1, user.skills["Chaos Magic"].value * 3, "blasts", "Chaos Magic")
        totalDamage = CombatEngine.SwingWeapon(user, target, tempWeapon, False)
        target.DamageMe(totalDamage)
        ActionLog.instance.AddText(totalDamage, user, target, tempWeapon)
        user.DecreaseMana(self.manaCost)