from Abilities.Abstracts import AbstractAbility
from JoyLib.Code.Entities.Abilities import AbilityTrigger
from JoyLib.Code.Entities.Abilities import AbilityTarget
from JoyLib.Code.Combat import CombatEngine
from JoyLib.Code.States import WorldState

class Throw(AbstractAbility.AbstractAbility):
    def __init__(self):
        self.abilityTrigger = AbilityTrigger.OnUse
        self.target = AbilityTarget.Ranged
        self.stacking = False
        self.name = "Throw"
        self.internalName = "Throw"
        self.description = "Throw an item."
        self.file = __file__
        self.counter = 1
        self.magnitude = 1
        self.priority = 1
        self.manaCost = 0

    def Use(self, user, target, ability):
        userWeapon = user.GetEquipment("Throw")
        totalDamage = CombatEngine.SwingWeapon(user, target, userWeapon)
        target.DamageMe(totalDamage)
        user.RemoveEquipmentToBackpack("Throw")
        user.PlaceItemInWorld(userWeapon)
        userWeapon.Move(target.position)