from Abilities.Abstracts import AbstractAbility
from JoyLib.Code.Entities.Abilities import AbilityTrigger
from JoyLib.Code.Entities.Abilities import AbilityTarget
from JoyLib.Code.Combat import CombatEngine
from JoyLib.Code.Helpers import ActionLog

class PowerAttack(AbstractAbility.AbstractAbility):
    def __init__(self):
        self.abilityTrigger = AbilityTrigger.OnUse
        self.target = AbilityTarget.Adjacent
        self.stacking = False
        self.name = "Power Attack"
        self.internalName = "PowerAttack"
        self.description = "Swing harder than you've ever swung before."
        self.file = __file__
        self.counter = 1
        self.magnitude = 1
        self.priority = 1
        self.manaCost = 5

    def Use(self, user, target, ability):
        userWeapon = user.GetEquipment("Hand1")
        totalDamage = CombatEngine.SwingWeapon(user, target, userWeapon, False) * 2
        target.DamageMe(totalDamage)
        ActionLog.instance.AddText(totalDamage, user, target, userWeapon)
        user.DecreaseMana(self.manaCost)