#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Arrowgene.Ddon.GameServer.Characters;
using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;

namespace Arrowgene.Ddon.GameServer.Handler
{
    public class CraftStartEquipGradeUpHandler : GameRequestPacketHandler<C2SCraftStartEquipGradeUpReq, S2CCraftStartEquipGradeUpRes>
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(CraftStartEquipGradeUpHandler));
        private readonly ItemManager _itemManager;

        public CraftStartEquipGradeUpHandler(DdonGameServer server) : base(server)
        {
            _itemManager = Server.ItemManager;
        }

        public override S2CCraftStartEquipGradeUpRes Handle(GameClient client, C2SCraftStartEquipGradeUpReq request)
        {
            string equipItemUID = request.EquipItemUID;
            Character character = client.Character;
            var ramItem = character.Storage.FindItemByUIdInStorage(ItemManager.EquipmentStorages, equipItemUID);
            var equipItem = ramItem.Item2.Item2;
            uint charid = client.Character.CharacterId;
            uint craftpawnid = request.CraftMainPawnID;

            // Fetch the crafting recipe data for the item
            CDataMDataCraftGradeupRecipe recipeData = Server.AssetRepository.CraftingGradeUpRecipesAsset
                .SelectMany(recipes => recipes.RecipeList)
                .First(recipe => recipe.ItemID == equipItem.ItemId);

            uint gearUpgradeID = recipeData.GradeupItemID;
            uint goldRequired = recipeData.Cost;
            UpgradableStatus upgradableStatus = recipeData.Upgradable;
            uint pawnExp = recipeData.Exp;
            bool canContinue = true;
            bool isGreatSuccess = Random.Shared.Next(5) == 0;
            bool doUpgrade = false;
            uint currentTotalEquipPoint = equipItem.EquipPoints;

            CDataCurrentEquipInfo CurrentEquipInfo = new()
            {
                ItemUId = equipItemUID,
            };

            // More dummy data, looks like its DragonAugment related.
            CDataCraftStartEquipGradeUpUnk0Unk0 dragonAugmentData = new();
            CDataCraftStartEquipGradeUpUnk0 dummydata = new() // TODO: Figure this out
            {
                Unk0 = new List<CDataCraftStartEquipGradeUpUnk0Unk0> { dragonAugmentData },
                DragonAugment = false, // makes the DragonAugment slot popup appear if set to true.
            };

            var res = new S2CCraftStartEquipGradeUpRes();
            S2CItemUpdateCharacterItemNtc updateCharacterItemNtc = new();

            // Removes crafting materials
            foreach (var craftMaterial in request.CraftMaterialList)
            {
                try
                {
                    var updateResults =
                        _itemManager.ConsumeItemByUIdFromMultipleStorages(Server, client.Character, ItemManager.BothStorageTypes, craftMaterial.ItemUId, craftMaterial.ItemNum);
                    updateCharacterItemNtc.UpdateItemList.AddRange(updateResults);
                }
                catch (NotEnoughItemsException)
                {
                    throw new ResponseErrorException(ErrorCode.ERROR_CODE_ITEM_INVALID_ITEM_NUM, "Client Item Desync has Occurred.");
                }
            }

            // TODO: make use of pawn crafting skills instead
            uint addEquipPoint = (uint)((isGreatSuccess ? 300 : 180) * (0.8 + (Random.Shared.NextDouble() * 0.4)));

            currentTotalEquipPoint += addEquipPoint;

            // Subtract less Gold if support pawn is used and add slightly more points
            if (request.CraftSupportPawnIDList.Count > 0)
            {
                goldRequired = (uint)(goldRequired * 0.95);
                currentTotalEquipPoint = (uint)(currentTotalEquipPoint * 1.5); // Fake stuff until pawn craft levels
            }

            var updateWalletPoint = Server.WalletManager.RemoveFromWallet(client.Character, WalletType.Gold, goldRequired);
            updateCharacterItemNtc.UpdateWalletList.Add(updateWalletPoint);

            ClientItemInfo itemInfo = ClientItemInfo.GetInfoForItemId(Server.AssetRepository.ClientItemInfos, equipItem.ItemId);
            byte currentStars = (byte)itemInfo.Quality;
            uint remainingPoints = currentTotalEquipPoint;
            List<CDataCommonU32> gradeupList = new List<CDataCommonU32>();

            uint[] thresholds = { 350, 700, 1000, 1500, 800 };

            // Determine the required points based on the current star level
            int requiredPoints = currentStars >= 0 && currentStars < thresholds.Length
                ? (int)thresholds[currentStars]
                : throw new InvalidOperationException("Invalid star level");

            if (recipeData.AllowMultiGrade)
            {
                if (currentTotalEquipPoint >= requiredPoints)
                {
                    doUpgrade = true;
                    List<CDataMDataCraftGradeupRecipe> itemIDsList = FindRecipeFamily(recipeData);
                    remainingPoints = currentTotalEquipPoint;
                    int thresholdsExceeded = 0;

                    for (int i = currentStars; i < thresholds.Length; i++)
                    {
                        if (remainingPoints >= thresholds[i])
                        {
                            remainingPoints -= thresholds[i];
                            thresholdsExceeded++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    gradeupList = itemIDsList.Take(thresholdsExceeded).Select(recipe => new CDataCommonU32(recipe.GradeupItemID)).ToList();
                    upgradableStatus = itemIDsList.Take(thresholdsExceeded).LastOrDefault().Upgradable;
                    gearUpgradeID = gradeupList.Count > 0 ? gradeupList.Last().Value : 0;
                }
            }
            else
            {
                if (currentTotalEquipPoint >= requiredPoints)
                {
                    int nextThresholdIndex = currentStars + 1;
                    if (nextThresholdIndex < thresholds.Length && remainingPoints >= thresholds[nextThresholdIndex])
                    {
                        // Cap the remainingPoints to 1 point short of the next threshold
                        remainingPoints = thresholds[nextThresholdIndex] - 1;
                    }

                    gradeupList = new() { new CDataCommonU32(gearUpgradeID) };
                    doUpgrade = true;
                }
            }


            if (upgradableStatus == UpgradableStatus.No) // This should handle a "True" state because I pull it from the Recipe directly.
            {
                canContinue = false;
            }

            if (doUpgrade)
            {
                equipItem.ItemId = gearUpgradeID;
                if (canContinue)
                {
                    currentTotalEquipPoint = remainingPoints;
                }
                else
                {
                    currentTotalEquipPoint = 0;
                }

                equipItem.EquipPoints = currentTotalEquipPoint;
                Server.Database.UpdateItemEquipPoints(equipItemUID, currentTotalEquipPoint);
                UpdateCharacterItem(client, equipItemUID, equipItem, charid, updateCharacterItemNtc, CurrentEquipInfo);
                res = CreateUpgradeResponse(equipItemUID, gearUpgradeID, gradeupList, addEquipPoint, currentTotalEquipPoint, (uint)upgradableStatus, goldRequired, isGreatSuccess,
                    CurrentEquipInfo, equipItem.ItemId, canContinue, dummydata);
            }
            else
            {
                equipItem.ItemId = equipItem.ItemId;
                equipItem.EquipPoints = currentTotalEquipPoint;
                Server.Database.UpdateItemEquipPoints(equipItemUID, currentTotalEquipPoint);
                res = CreateEquipPointResponse(equipItemUID, addEquipPoint, currentTotalEquipPoint, goldRequired, isGreatSuccess, CurrentEquipInfo, canContinue, dummydata);
            }

            // Lead pawn is always owned by player.
            Pawn leadPawn = Server.CraftManager.FindPawn(client, request.CraftMainPawnID);
            if (CraftManager.CanPawnExpUp(leadPawn))
            {
                CraftManager.HandlePawnExpUpNtc(client, leadPawn, pawnExp, 0);
                if (CraftManager.CanPawnRankUp(leadPawn))
                {
                    CraftManager.HandlePawnRankUpNtc(client, leadPawn);
                }
                Server.Database.UpdatePawnBaseInfo(leadPawn);
            }

            client.Send(updateCharacterItemNtc);
            return res;
        }

        private void UpdateCharacterItem(GameClient client, string equipItemUID, Item equipItem, uint charid, S2CItemUpdateCharacterItemNtc updateCharacterItemNtc,
            CDataCurrentEquipInfo CurrentEquipInfo)
        {
            var (storageType, foundItem) = client.Character.Storage.FindItemByUIdInStorage(ItemManager.EquipmentStorages, equipItemUID);
            if (foundItem != null)
            {
                var (slotno, item, itemnum) = foundItem;
                CharacterCommon characterCommon = null;
                if (storageType == StorageType.CharacterEquipment || storageType == StorageType.PawnEquipment)
                {
                    CurrentEquipInfo.EquipSlot.EquipSlotNo = EquipManager.DetermineEquipSlot(slotno);
                    CurrentEquipInfo.EquipSlot.EquipType = EquipManager.GetEquipTypeFromSlotNo(slotno);
                }

                if (storageType == StorageType.PawnEquipment)
                {
                    uint pawnId = Storages.DeterminePawnId(client.Character, storageType, slotno);
                    CurrentEquipInfo.EquipSlot.PawnId = pawnId;
                    characterCommon = client.Character.Pawns.SingleOrDefault(x => x.PawnId == pawnId);
                }
                else if (storageType == StorageType.CharacterEquipment)
                {
                    CurrentEquipInfo.EquipSlot.CharacterId = charid;
                    characterCommon = client.Character;
                }

                updateCharacterItemNtc.UpdateType = ItemNoticeType.StartEquipGradeUp;
                updateCharacterItemNtc.UpdateItemList.Add(Server.ItemManager.CreateItemUpdateResult(characterCommon, equipItem, storageType, slotno, 0, 0));

                _itemManager.UpgradeStorageItem(Server, client, charid, storageType, equipItem, slotno);
                updateCharacterItemNtc.UpdateItemList.Add(Server.ItemManager.CreateItemUpdateResult(characterCommon, equipItem, storageType, slotno, 1, 1));
            }
            else
            {
                throw new ResponseErrorException(ErrorCode.ERROR_CODE_ITEM_INVALID_STORAGE_TYPE, $"Item with UID {equipItemUID} not found in {storageType}");
            }
        }

        private S2CCraftStartEquipGradeUpRes CreateUpgradeResponse(string equipItemUID, uint gradeUpItemID, List<CDataCommonU32> gradeupList,
            uint addEquipPoint, uint currentTotalEquipPoint, uint canUpgrade, uint gold, bool isGreatSuccess, CDataCurrentEquipInfo currentEquip,
            uint beforeItemID, bool upgradable, CDataCraftStartEquipGradeUpUnk0 unk1)
        {
            return new S2CCraftStartEquipGradeUpRes
            {
                GradeUpItemUID = equipItemUID,
                GradeUpItemID = gradeUpItemID, // This has to be the last ID found in gradeupList or it will continue grading up into it.
                GradeUpItemIDList = gradeupList, // Only assign this when its meant to become the next item, or it will autofill the gauge everytime.
                AddEquipPoint = addEquipPoint,
                TotalEquipPoint = currentTotalEquipPoint,
                EquipGrade = canUpgrade,
                Gold = gold,
                IsGreatSuccess = isGreatSuccess,
                CurrentEquip = currentEquip,
                BeforeItemID = beforeItemID,
                Upgradable = upgradable,
                Unk1 = unk1 // Dragon Augment related I guess?
            };
        }

        private S2CCraftStartEquipGradeUpRes CreateEquipPointResponse(string equipItemUID, uint addEquipPoint, uint totalEquipPoint, uint gold, bool isGreatSuccess,
            CDataCurrentEquipInfo currentEquip, bool upgradable, CDataCraftStartEquipGradeUpUnk0 unk1)
        {
            return new S2CCraftStartEquipGradeUpRes
            {
                GradeUpItemUID = equipItemUID,
                AddEquipPoint = addEquipPoint,
                TotalEquipPoint = totalEquipPoint,
                Gold = gold,
                IsGreatSuccess = isGreatSuccess,
                CurrentEquip = currentEquip,
                Upgradable = upgradable,
                Unk1 = unk1 // Dragon Augment related I guess?
            };
        }

        private List<CDataMDataCraftGradeupRecipe> FindRecipeFamily(CDataMDataCraftGradeupRecipe startingRecipe)
        {
            List<CDataMDataCraftGradeupRecipe> recipeFamily = new List<CDataMDataCraftGradeupRecipe>();
            recipeFamily.Add(startingRecipe);
            CDataMDataCraftGradeupRecipe? node = startingRecipe;
            //Search forwards
            while (node is not null)
            {
                node = Server.AssetRepository.CraftingGradeUpRecipesAsset
                    .SelectMany(recipes => recipes.RecipeList)
                    .Where(x => x.ItemID == node.GradeupItemID)
                    .FirstOrDefault();

                if (node is not null)
                {
                    recipeFamily.Add(node);
                }
            }

            recipeFamily = recipeFamily.OrderBy(x => x.RecipeID).ToList();
            //recipeFamily.ForEach(x => Logger.Debug($"Found recipe family: {startingRecipe.RecipeID} -> {x.RecipeID}"));
            return recipeFamily;
        }
    }
}
