from Abilities.Abstracts import AbstractAbility
from JoyLib.Code.Entities.Abilities import AbilityTrigger
from JoyLib.Code.Entities.Abilities import AbilityTarget
from JoyLib.Code.Combat import CombatEngine
from JoyLib.Code.Helpers import ActionLog

class Backdraft(AbstractAbility.AbstractAbility):
    def __init__(self):
        self.abilityTrigger = AbilityTrigger.OnUse
        self.target = AbilityTarget.Adjacent
        self.stacking = False
        self.name = "Backdraft"
        self.internalName = "Backdraft"
        self.description = "A bone-crushing attack that damages the user as well as the target."
        self.file = __file__
        self.counter = 1
        self.magnitude = 1
        self.priority = 1
        self.manaCost = 8

    def Use(self, user, target, ability):
        userWeapon = user.GetEquipment("Hand1")
        totalDamage = CombatEngine.SwingWeapon(user, target, userWeapon, False)
        target.DamageMe(totalDamage * 4)
        ActionLog.instance.AddText(totalDamage * 4, user, target, userWeapon)
        user.DamageMe(totalDamage)
        ActionLog.instance.AddText(totalDamage, user, user, userWeapon)
        user.DecreaseMana(self.manaCost)