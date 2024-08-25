using System;
using System.Collections.Generic;
using System.Linq;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Model;

namespace Arrowgene.Ddon.GameServer.Characters
{
    public class CraftCalculationResult
    {
        public uint CalculatedValue { get; set; }
        public bool IsGreatSuccess { get; set; }
    }

    public class CraftManager
    {
        // TODO: introduce new assets instead
        private static readonly List<uint> craftRankExpLimit = new()
        {
            0, 8, 120, 240, 400, 800, 1400, 2150, 3050, 4100, 5300, 6600, 8000, 9500, 11000, 13000, 15000, 17500, 20000, 23000, 27000, 29500, 32500, 36000, 39500, 43500, 47000,
            51000, 55000, 60000, 63000, 67461, 72101, 76927, 81946, 87165, 92593, 98239, 104110, 110216, 116566, 123170, 130039, 137182, 144611, 152337, 160372, 168728, 177419,
            186457, 195857, 205633, 215800, 226374, 237370, 248807, 260701, 273070, 285935, 299314, 313228, 327699, 342748, 358400, 374678, 391606, 409212, 427522, 446565, 466369
        };

        private static readonly Dictionary<uint, uint> craftRankLimitPromotionRecipes = new()
        {
            { 8, 1406 }, { 16, 1488 }, { 21, 1490 }, { 26, 1523 }, { 31, 1841 }, { 36, 1903 }, { 41, 2282 }, { 46, 2283 }, { 51, 2466 }, { 56, 2680 }, { 61, 2826 }, { 66, 2949 }
        };

        private static readonly Dictionary<uint, uint> craftRankLimits = new()
        {
            { 8, 16 }, { 16, 21 }, { 21, 26 }, { 26, 31 }, { 31, 36 }, { 36, 41 }, { 41, 46 }, { 46, 51 }, { 51, 56 }, { 56, 61 }, { 61, 66 }, { 66, 71 }
        };

        private const int GreatSuccessOddsDefault = 10;
        private const uint CraftSkillLevelMax = 70;
        private const uint PawnCraftRankMaxLimit = 70;
        private const double CraftPawnsMax = 4;

        /// <summary>
        /// Randomly chosen maximum of 50% reduction rate.
        /// Based on client behavior reduction can not go below a specific unknown threshold for each recipe, e.g. 45sec -> 30sec, but 60sec -> 30sec.
        /// </summary>
        private const uint ProductionSpeedMaximumTotal = 50;

        /// <summary>
        /// Minimum of 4% reduction added per pawn based on video evidence.
        /// </summary>
        private const uint ProductionSpeedMinimumPerPawn = 4;

        private const double ProductionSpeedIncrementPerLevel = (ProductionSpeedMaximumTotal / CraftPawnsMax - ProductionSpeedMinimumPerPawn) / CraftSkillLevelMax;

        /// <summary>
        /// Randomly chosen maximum of 50% chance.
        /// </summary>  
        private const uint EquipmentQualityMaximumTotal = 50;

        /// <summary>
        /// Randomly chosen minimum of 4% chance added per pawn.
        /// </summary>
        private const uint EquipmentQualityMinimumPerPawn = 4;

        private const double EquipmentQualityIncrementPerLevel = (EquipmentQualityMaximumTotal / CraftPawnsMax - EquipmentQualityMinimumPerPawn) / CraftSkillLevelMax;

        /// <summary>
        /// Minimum enhancement points at 150 total based on video evidence.
        /// </summary>
        private const uint EquipmentEnhancementMinimumTotal = 150;

        /// <summary>
        /// Maximum per pawn at 95 roughly based on maximum points observed based on video evidence, represents delta to minimum divided by max pawn count of 4.
        /// </summary>
        private const double EquipmentEnhancementMaximumPerPawn = 95;

        /// <summary>
        /// Great success factor 1.2 chosen based on Wiki and video evidence.
        /// </summary>
        private const double EquipmentEnhancementGreatSuccessFactor = 1.2;

        private const double EquipmentEnhancementIncrementPerLevel = EquipmentEnhancementMaximumPerPawn / CraftSkillLevelMax;

        /// <summary>
        /// Randomly chosen maximum quantity of 1 consumable each pawn can contribute. 
        /// </summary>
        private const uint ConsumableQuantityMaximumQuantityPerPawn = 1;

        /// <summary>
        /// Randomly chosen maximum of 50% chance.
        /// </summary>
        private const uint ConsumableQuantityMaximumTotal = 50;

        /// <summary>
        /// Randomly chosen minimum of 4% chance added per pawn.
        /// </summary>
        private const uint ConsumableQuantityMinimumPerPawn = 4;

        private const double ConsumableQuantityIncrementPerLevel = (ConsumableQuantityMaximumTotal / CraftPawnsMax - ConsumableQuantityMinimumPerPawn) / CraftSkillLevelMax;

        /// <summary>
        /// Randomly chosen maximum of 50% reduction.
        /// </summary>
        private const uint CostPerformanceMaximumTotal = 50;

        /// <summary>
        /// Randomly chosen minimum of 4% reduction added per pawn.
        /// </summary>
        private const uint CostPerformanceMinimumPerPawn = 4;

        private const double CostPerformanceIncrementPerLevel = (CostPerformanceMaximumTotal / CraftPawnsMax - CostPerformanceMinimumPerPawn) / CraftSkillLevelMax;

        private readonly DdonGameServer _server;

        public CraftManager(DdonGameServer server)
        {
            _server = server;
        }

        #region production speed

        /// <summary>
        /// Calculates crafting time reduction based on an increment per level and ensures a minimum is added for each pawn that is present.
        /// Can be manipulated using server setting AdditionalProductionSpeedFactor, which at 1.0 has no effect.
        /// </summary>
        /// <param name="productionSpeedLevels"></param>
        /// <returns></returns>
        public double GetCraftingTimeReductionRate(List<uint> productionSpeedLevels)
        {
            return Math.Clamp(
                productionSpeedLevels.Select(level => level * ProductionSpeedIncrementPerLevel + ProductionSpeedMinimumPerPawn).Sum() *
                _server.Setting.GameLogicSetting.AdditionalProductionSpeedFactor, 0, 100);
        }

        public uint CalculateRecipeProductionSpeed(uint recipeTime, List<uint> productionSpeedLevels)
        {
            // TODO: Figure out actual formula + lower/upper bounds client uses
            double productionSpeedFactor = (100 - GetCraftingTimeReductionRate(productionSpeedLevels)) / 100;
            return (uint)Math.Clamp(recipeTime * productionSpeedFactor, 0, recipeTime);
        }

        #endregion

        #region equipment quality

        public static double GetEquipmentQualityIncreaseRate(List<uint> equipmentQualityLevels)
        {
            return Math.Clamp(equipmentQualityLevels.Select(level => level * EquipmentQualityIncrementPerLevel + EquipmentQualityMinimumPerPawn).Sum(), 0, 100);
        }

        /// <summary>
        /// Based on previously existing quality calculations. Currently, does not make use of quality skill levels.
        /// </summary>
        /// <param name="refineMaterialItem"></param>
        /// <param name="equipmentQualityLevels"></param>
        /// <returns></returns>
        public static CraftCalculationResult CalculateEquipmentQuality(Item refineMaterialItem, List<uint> equipmentQualityLevels)
        {
            // TODO: Figure out actual formula + lower/upper bounds client uses
            // Based on season 1 evidence:
            //  3x lvl 45 1x lvl 44 => 79% 
            // 1x lvl44, 3x lvl 1 => 63

            byte greatSuccessValue = 1;
            int greatSuccessOdds = GreatSuccessOddsDefault;
            byte RandomQuality = 0;

            if (refineMaterialItem != null)
            {
                switch (refineMaterialItem.ItemId)
                {
                    // Quality Rocks (Tier2)
                    case 8036 or 8068:
                        RandomQuality = 2;
                        greatSuccessValue = 3;
                        break;
                    // WhiteDragon Rocks (Tier3)
                    case 8052 or 8084:
                        RandomQuality = 2;
                        greatSuccessValue = 3;
                        greatSuccessOdds = 5;
                        break;
                    // Standard Rocks (Tier1)
                    case 8035 or 8067:
                        RandomQuality = 1;
                        greatSuccessValue = 2;
                        break;
                }
            }

            var isGreatSuccess = CalculateIsGreatSuccess(greatSuccessOdds);

            if (isGreatSuccess)
            {
                RandomQuality = greatSuccessValue;
            }

            return new CraftCalculationResult()
            {
                CalculatedValue = RandomQuality,
                IsGreatSuccess = isGreatSuccess
            };
        }

        #endregion

        #region equipment enhancement

        public static double GetEquipmentEnhancementPoints(List<uint> equipmentEnhancementLevels)
        {
            return EquipmentEnhancementMinimumTotal + equipmentEnhancementLevels.Select(level => level * EquipmentEnhancementIncrementPerLevel).Sum();
        }

        public static double GetEquipmentEnhancementPointsGreatSuccess(List<uint> equipmentEnhancementLevels)
        {
            return GetEquipmentEnhancementPoints(equipmentEnhancementLevels) * EquipmentEnhancementGreatSuccessFactor;
        }

        public CraftCalculationResult CalculateEquipmentEnhancement(List<uint> equipmentEnhancementLevels)
        {
            // TODO: Figure out actual formula + lower/upper bounds client uses
            // Based on season 1 evidence:
            //  4x lvl 45 => min 390 / max 468 pt
            //  1x lvl 45 + 3x lvl 1 => min 266 / max 319 pt
            // According to wikis: 150 + (levelValue - 1 ) * 1.73 => mostly season 1

            double equipmentEnhancementPoints = GetEquipmentEnhancementPoints(equipmentEnhancementLevels);
            bool isGreatSuccess = CalculateIsGreatSuccess();
            equipmentEnhancementPoints *= isGreatSuccess ? EquipmentEnhancementGreatSuccessFactor : 1;
            return new CraftCalculationResult()
            {
                CalculatedValue = (uint)equipmentEnhancementPoints,
                IsGreatSuccess = isGreatSuccess
            };
        }

        #endregion

        #region consumable quantity

        public static double GetAdditionalConsumableQuantityRate(List<uint> consumableQuantityLevels)
        {
            return Math.Clamp(consumableQuantityLevels.Select(level => level * ConsumableQuantityIncrementPerLevel + ConsumableQuantityMinimumPerPawn).Sum(), 0, 100);
        }

        public static double GetAdditionalConsumableQuantityMaximum(List<uint> consumableQuantityLevels)
        {
            return consumableQuantityLevels.Count * ConsumableQuantityMaximumQuantityPerPawn;
        }

        /// <summary>
        /// Takes craft rank and craft skill level of all pawns into account and allows to push the minimum chance to 50% to add up to 3 additional items.
        /// </summary>
        public static CraftCalculationResult CalculateConsumableQuantity(List<uint> consumableQuantityLevels)
        {
            // TODO: Figure out actual formula + lower/upper bounds client uses
            double consumableQuantityChance = GetAdditionalConsumableQuantityRate(consumableQuantityLevels);
            uint quantity = 0;
            for (int i = 0; i < consumableQuantityLevels.Count; i++)
            {
                quantity += Random.Shared.Next(100) < consumableQuantityChance ? ConsumableQuantityMaximumQuantityPerPawn : 0;
            }

            bool isGreatSuccess = CalculateIsGreatSuccess();
            if (isGreatSuccess)
            {
                quantity++;
            }

            return new CraftCalculationResult
            {
                CalculatedValue = quantity,
                IsGreatSuccess = isGreatSuccess
            };
        }

        #endregion

        #region cost performance

        public double GetCraftCostReductionRate(List<uint> costPerformanceLevels)
        {
            return Math.Clamp(
                costPerformanceLevels.Select(level => level * CostPerformanceIncrementPerLevel + CostPerformanceMinimumPerPawn).Sum() *
                _server.Setting.GameLogicSetting.AdditionalCostPerformanceFactor, 0, 100);
        }

        /// <summary>
        /// Takes craft skill level of all pawns into account and allows for a maximum of 50% reduction by default.
        /// </summary>
        /// <param name="recipeCost"></param> Original item recipe's crafting cost.
        /// <param name="costPerformanceLevels"></param> List of cost performance craft skill levels for involved pawns.
        /// <returns></returns>
        public uint CalculateRecipeCost(uint recipeCost, List<uint> costPerformanceLevels)
        {
            // TODO: Figure out actual formula + lower/upper bounds client uses
            // Based on season 1 evidence:
            //  1x lvl 2, 3x lvl 1 == 16% maximum
            double costPerformanceFactor = (100 - GetCraftCostReductionRate(costPerformanceLevels)) / 100;
            return (uint)Math.Clamp(recipeCost * costPerformanceFactor, 0, recipeCost);
        }

        #endregion

        public static bool CalculateIsGreatSuccess(int odds = GreatSuccessOddsDefault)
        {
            return Random.Shared.Next(odds) == 0;
        }

        public static uint GetPawnProductionSpeedLevel(Pawn pawn)
        {
            return GetPawnCraftLevel(pawn, CraftSkillType.ProductionSpeed);
        }

        public static uint GetPawnEquipmentEnhancementLevel(Pawn pawn)
        {
            return GetPawnCraftLevel(pawn, CraftSkillType.EquipmentEnhancement);
        }

        public static uint GetPawnEquipmentQualityLevel(Pawn pawn)
        {
            return GetPawnCraftLevel(pawn, CraftSkillType.EquipmentQuality);
        }

        public static uint GetPawnConsumableQuantityLevel(Pawn pawn)
        {
            return GetPawnCraftLevel(pawn, CraftSkillType.ConsumableQuantity);
        }

        public static uint GetPawnCostPerformanceLevel(Pawn pawn)
        {
            return GetPawnCraftLevel(pawn, CraftSkillType.CostPerformance);
        }

        public static uint GetPawnCraftLevel(Pawn pawn, CraftSkillType craftSkillType)
        {
            return pawn.CraftData.PawnCraftSkillList.Find(skill => skill.Type == craftSkillType).Level;
        }

        public static bool IsCraftRankLimitPromotionRecipe(Pawn pawn, uint recipeId)
        {
            return craftRankLimitPromotionRecipes.ContainsKey(pawn.CraftData.CraftRank) && craftRankLimitPromotionRecipes[pawn.CraftData.CraftRank] == recipeId;
        }

        public static void PromotePawnRankLimit(Pawn pawn)
        {
            if (craftRankLimitPromotionRecipes.ContainsKey(pawn.CraftData.CraftRank))
            {
                pawn.CraftData.CraftRankLimit = craftRankLimits[pawn.CraftData.CraftRankLimit];
            }
        }

        public static bool CanPawnRankUp(Pawn pawn)
        {
            return pawn.CraftData.CraftExp >= craftRankExpLimit[(int)pawn.CraftData.CraftRank] && pawn.CraftData.CraftRank < pawn.CraftData.CraftRankLimit;
        }

        public static uint CalculatePawnRankUp(Pawn pawn)
        {
            uint rankUps = 0;

            if (CanPawnRankUp(pawn))
            {
                for (int i = (int)pawn.CraftData.CraftRank; i < pawn.CraftData.CraftRankLimit; i++)
                {
                    if (pawn.CraftData.CraftExp >= craftRankExpLimit[i])
                    {
                        rankUps++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return rankUps;
        }

        public static void HandlePawnRankUpNtc(GameClient client, Pawn leadPawn)
        {
            S2CCraftCraftRankUpNtc rankUpNtc = new S2CCraftCraftRankUpNtc
            {
                PawnId = leadPawn.PawnId,
                CraftRank = leadPawn.CraftData.CraftRank
            };
            
            uint rankUps = CalculatePawnRankUp(leadPawn);
            if (rankUps > 0)
            {
                uint rankUpDelta = Math.Clamp(leadPawn.CraftData.CraftRank + rankUps, leadPawn.CraftData.CraftRank, PawnCraftRankMaxLimit) - leadPawn.CraftData.CraftRank;
                
                leadPawn.CraftData.CraftRank += rankUpDelta;
                leadPawn.CraftData.CraftPoint += rankUpDelta;

                rankUpNtc.AddCraftPoints = rankUpDelta;
                rankUpNtc.CraftRank = leadPawn.CraftData.CraftRank;
                rankUpNtc.TotalCraftPoint = leadPawn.CraftData.CraftPoint;
                
                leadPawn.CraftData.CraftExp = Math.Clamp(leadPawn.CraftData.CraftExp, 0, craftRankExpLimit[(int)leadPawn.CraftData.CraftRankLimit-1]);
            }

            client.Send(rankUpNtc);
        }

        public static bool CanPawnExpUp(Pawn pawn)
        {
            return pawn.CraftData.CraftRank < pawn.CraftData.CraftRankLimit;
        }

        public static void HandlePawnExpUpNtc(GameClient client, Pawn leadPawn, uint exp, uint bonusExp)
        {
            S2CCraftCraftExpUpNtc expNtc = new S2CCraftCraftExpUpNtc()
            {
                PawnId = leadPawn.PawnId,
                CraftRankLimit = leadPawn.CraftData.CraftRankLimit
            };
            
            if (CanPawnExpUp(leadPawn))
            {
                expNtc.AddExp = exp;
                expNtc.ExtraBonusExp = bonusExp;
                expNtc.TotalExp = exp + bonusExp;
                
                leadPawn.CraftData.CraftExp += expNtc.TotalExp;
                
                leadPawn.CraftData.CraftExp = Math.Clamp(leadPawn.CraftData.CraftExp, 0, craftRankExpLimit[(int)leadPawn.CraftData.CraftRankLimit]);
            }

            client.Send(expNtc);
        }

        public Pawn FindPawn(GameClient client, uint pawnId)
        {
            Pawn pawn = client.Character.Pawns.Find(p => p.PawnId == pawnId);
            if (pawn == null)
            {
                pawn = _server.Database.SelectPawn(pawnId);
            }

            return pawn;
        }
    }
}
