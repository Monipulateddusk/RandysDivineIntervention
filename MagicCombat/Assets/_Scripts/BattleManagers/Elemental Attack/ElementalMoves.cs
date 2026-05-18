using TurnBased.AttackResolution;

namespace TurnBased
{
    public interface IElementalMove
    {
        public abstract AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData);
        public abstract string GetMoveName();
        public abstract UnitTargetSelectorType GetTargetSelectorType();
        public string GetElementalMoveDescription();
        public MoveResolutionTiming GetResolutionTiming();
    }

    #region Fire Moves

    /// <summary>
    /// This move is when the fire element combines with another fire element. It deals high fire damage to all targets based on all fire user's magic power and level 
    /// </summary>
    public class EM_Inferno : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalFireAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.FIRE);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                        
                            // Afflicting status: Burned to enemies
                            new AttackResolution.ElementalDamageAttackAction(Element.FIRE, totalFireAttack, 1,  groupID: 0)
                        }
                    }
                }
            };
            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;

        public string GetElementalMoveDescription() => "Deals a High amount of Fire Damage to the Highest Health Enemy proportional to the Strength of Fire-Elemental Users.";

        public string GetMoveName() => "Inferno";

        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;
    }

    /// <summary>
    /// This move is when the fire element combines with a water element. It deals medium water damage and afflicts targets with burning (deals 1 hp per turn)
    /// </summary>
    public class EM_Steam : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalWaterAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.WATER);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.Area) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                        
                            // Afflicting status: Burned to enemies
                            new AttackResolution.ElementalDamageAttackAction(Element.WATER, totalWaterAttack, 1, groupID: 0)
                        }
                    }
                }

            };
            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Water Damage to the Highest Health Enemy proportional to the Strength of Water-Elemental Users.";

        public string GetMoveName() => "Steam";
    }

    /// <summary>
    /// This move is when the fire element combines with an ice element. It afflicts the target with two status; frostburn and slippery
    /// </summary>
    public class EM_Frostburn : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                        
                            // Afflicting status: Frostburn to enemies & Slippery
                           // new AttackAction(AttackActionType.DAMAGE, elementEff: Element.WATER, staEffect: "Frostburn", attackTarget: MoveTarget.SingleEnemy)
                            new AttackResolution.ElementalDamageAttackAction(Element.WATER, 3, 1,  groupID: 0)
                        }
                    }

                }
            };
            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Ice Damage to the Highest Health Enemy proportional to the Strength of Ice-Elemental Users.";

        public string GetMoveName() => "Frostburn";
    }

    /// <summary>
    /// This move is when the fire element combines with an earth element. It deals medium earth damage and inflicts targets with burning status dealing 1 damage per turn 
    /// </summary>
    public class EM_Volcano : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalEarthAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.EARTH);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0,  MoveTarget.SingleEnemy)},

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                        
                            // Afflicting status: Burned to enemies
                           // new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, staEffect: "Burned", attackTarget: MoveTarget.SingleEnemy)
                           new AttackResolution.ElementalDamageAttackAction(Element.EARTH, 1, totalEarthAttack,  groupID: 0)
                        }
                    }

                }
            };

            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Earth Damage to the Highest Health Enemy proportional to the Strength of Earth-Elemental Users.";

        public string GetMoveName() => "Volcano";
    }

    #endregion

    #region Water Moves

    /// <summary>
    /// This move is when the water element combines with another water element. It deals high water damage to target
    /// </summary>
    public class EM_Tsunami : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalWaterAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.WATER);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.AllAllies) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackResolution.HealingAttackAction (totalWaterAttack, groupID: 0)
                        }
                    }

                }
            };

            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Water Damage to the Highest Health Enemy proportional to the Strength of Water-Elemental Users.";

        public string GetMoveName() => "Tsunami";
    }

    /// <summary>
    /// This move is when the water element combines with an earth element. Allies are healed for 2 points of HP
    /// </summary>
    public class EM_Wellspring : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.AllAllies) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            // All allies are healed for 2 HP
                            new AttackResolution.HealingAttackAction(2, groupID: 0)
                        }
                    }

                }

            };

            return resolutionInfo;
        }
        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Heals all Allies for 2 Health Points.";

        public string GetMoveName() => "Wellspring";

    }

    #endregion

    #region Ice Moves

    /// <summary>
    /// This move is when the ice element combines with another ice element. It deals high ice damage to all targets based on all ice user's magic power and level 
    /// </summary>
    public class EM_IceAge : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalIceAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.ICE);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            //new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.ICE, attackTarget: MoveTarget.SingleEnemy)
                            new AttackResolution.ElementalDamageAttackAction(Element.ICE, 1, totalIceAttack,  groupID: 0)
                        }
                    }

                }
            };
            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Ice Damage to the Highest Health Enemy proportional to the Strength of Ice-Elemental Users.";

        public string GetMoveName() => "Ice Age";
    }

    /// <summary>
    /// This move is when the water element combines with an ice element. Allies are granted the status condition: Hail Cloak. (50% chance to not take damage) guarantees damage every other hit
    /// </summary>
    public class EM_HailCloak : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            // All allies are granted Hailcloak
                            //new AttackAction(AttackActionType.STATUS_EFFECT, staEffect: "Hail Cloak", attackTarget: MoveTarget.AllAllies)
                            new AttackResolution.ElementalDamageAttackAction(Element.ICE, 3, 1,  groupID: 0)
                        }
                    }

                }

            };
            return resolutionInfo;
        }
        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Ice Damage to the Highest Health Enemy proportional to the Strength of Ice-Elemental Users.";

        public string GetMoveName() => "Hail Cloak";
    }

    #endregion

    #region Earth Moves

    /// <summary>
    /// This move is when the earth element combines with another earth element. It deals high earth damage to all targets based on all earth user's magic power and level 
    /// </summary>
    public class EM_Fissure : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            int totalEarthAttack = ElementalMoveUtilities.GetTotalAttackValueOfUsersWithElement(resolutionSceneData.AllyUnitInformation, Element.EARTH);

            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            //new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, attackTarget: MoveTarget.SingleEnemy)
                            new AttackResolution.ElementalDamageAttackAction(Element.EARTH, totalEarthAttack, 1,  groupID: 0)
                        }
                    }

                }
            };


            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Earth Damage to the Highest Health Enemy proportional to the Strength of Earth-Elemental Users.";

        public string GetMoveName() => "Fissure";
    }

    /// <summary>
    /// This move is when the Earth element combines with an Ice element. It afflicts targets with the Frost Lock condition. Units cannot move (melee attackers skip their attack, ranged attackers can still attack)
    /// </summary>
    public class EM_FrostLock : IElementalMove
    {
        public AttackResolutionInfo ExecuteElementalMove(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            // Afflict targets with frost lock
                            //new AttackAction(AttackActionType.STATUS_EFFECT, staEffect: "Frost Lock", attackTarget: MoveTarget.AllEnemies)
                            new AttackResolution.ElementalDamageAttackAction(Element.EARTH, 3, 1,  groupID: 0)

                        }
                    }

                }
            };


            return resolutionInfo;
        }

        public UnitTargetSelectorType GetTargetSelectorType() => UnitTargetSelectorType.HighestHP;
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.Instant;

        public string GetElementalMoveDescription() => "Deals a High amount of Earth Damage to the Highest Health Enemy proportional to the Strength of Earth-Elemental Users.";

        public string GetMoveName() => "Frost Lock";
    }

    #endregion



    public static class ElementalMoveUtilities
    {
        public static int GetTotalAttackValueOfUsersWithElement(System.Collections.Generic.List<UnitInformation> usersInfo, Element element)
        {
            int totalAttackValue = 0;
            foreach (var unit in usersInfo)
            {
                if (unit.UnitElement == element)
                {
                    totalAttackValue += unit.UnitAttack;
                }
            }

            // Give a multiplier to the attack to make it better than the sum of it's parts
            return (int)(totalAttackValue * 1.5f);
        }
    }

}