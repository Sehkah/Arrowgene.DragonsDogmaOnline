﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;

namespace Arrowgene.Ddon.Shared.Csv
{
    public class EnemySpawnAssetDeserializer : IAssetDeserializer<EnemySpawnAsset>
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(EnemySpawnAssetDeserializer));

        private static readonly string[] ENEMY_HEADERS = new string[]{"StageId", "LayerNo", "GroupId", "SubGroupId", "EnemyId", "NamedEnemyParamsId", "RaidBossId", "Scale", "Lv", "HmPresetNo", "StartThinkTblNo", "RepopNum", "RepopCount", "EnemyTargetTypesId", "MontageFixNo", "SetType", "InfectionType", "IsBossGauge", "IsBossBGM", "IsManualSet", "IsAreaBoss", "BloodOrbs", "HighOrbs", "Experience", "DropsTableId"};
        private static readonly string[] DROPS_TABLE_HEADERS = new string[]{"ItemId", "ItemNum", "MaxItemNum", "Quality", "IsHidden", "DropChance"};

        public EnemySpawnAsset ReadPath(string path)
        {
            Logger.Info($"Reading {path}");

            EnemySpawnAsset asset = new EnemySpawnAsset();

            string json = File.ReadAllText(path);
            JsonDocument document = JsonDocument.Parse(json);

            JsonElement schemasElement = document.RootElement.GetProperty("schemas");
            List<string> dropsTablesItemsSchema = schemasElement.GetProperty("dropsTables.items").EnumerateArray().Select(element => element.GetString()).ToList();
            Dictionary<string, int> dropsTableSchemaIndexes = findSchemaIndexes(DROPS_TABLE_HEADERS, dropsTablesItemsSchema);
            List<string> enemiesSchema = schemasElement.GetProperty("enemies").EnumerateArray().Select(element => element.GetString()).ToList();
            Dictionary<string, int> enemySchemaIndexes = findSchemaIndexes(ENEMY_HEADERS, enemiesSchema);


            JsonElement dropsTablesElement = document.RootElement.GetProperty("dropsTables");
            foreach (JsonElement dropsTableElement in dropsTablesElement.EnumerateArray())
            {
                uint id = dropsTableElement.GetProperty("id").GetUInt32();
                DropsTable dropsTable = asset.DropsTables.GetValueOrDefault(id) ?? new DropsTable();
                dropsTable.Id = id;
                dropsTable.Name = dropsTableElement.GetProperty("name").GetString();
                dropsTable.MdlType = dropsTableElement.GetProperty("mdlType").GetByte();
                foreach (JsonElement dropsTableItemsRow in dropsTableElement.GetProperty("items").EnumerateArray())
                {
                    List<JsonElement> row = dropsTableItemsRow.EnumerateArray().ToList();
                    GatheringItem gatheringItem = new GatheringItem();
                    gatheringItem.ItemId = row[dropsTableSchemaIndexes["ItemId"]].GetUInt32();
                    gatheringItem.ItemNum = row[dropsTableSchemaIndexes["ItemNum"]].GetUInt32();
                    gatheringItem.MaxItemNum = row[dropsTableSchemaIndexes["MaxItemNum"]].GetUInt32();
                    gatheringItem.Quality = row[dropsTableSchemaIndexes["Quality"]].GetUInt32();
                    gatheringItem.IsHidden = row[dropsTableSchemaIndexes["IsHidden"]].GetBoolean();

                    // Optional for compatibility with older formats
                    if(dropsTableSchemaIndexes.ContainsKey("DropChance"))
                    {
                        gatheringItem.DropChance = row[dropsTableSchemaIndexes["DropChance"]].GetDouble();
                    }
                    else
                    {
                        gatheringItem.DropChance = 1;
                    }

                    dropsTable.Items.Add(gatheringItem);
                }

                asset.DropsTables[id] = dropsTable;
            }

            JsonElement enemiesElement = document.RootElement.GetProperty("enemies");
            foreach (JsonElement enemyRow in enemiesElement.EnumerateArray())
            {
                List<JsonElement> row = enemyRow.EnumerateArray().ToList();
                StageId layoutId = new StageId(
                    row[enemySchemaIndexes["StageId"]].GetUInt32(),
                    row[enemySchemaIndexes["LayerNo"]].GetByte(),
                    row[enemySchemaIndexes["GroupId"]].GetUInt32()
                );
                byte subGroupId = row[enemySchemaIndexes["SubGroupId"]].GetByte();
                List<Enemy> enemies = asset.Enemies.GetValueOrDefault((layoutId, subGroupId)) ?? new List<Enemy>();
                Enemy enemy = new Enemy()
                {
                    EnemyId = ParseHexUInt(row[enemySchemaIndexes["EnemyId"]].GetString()),
                    NamedEnemyParamsId = row[enemySchemaIndexes["NamedEnemyParamsId"]].GetUInt32(),
                    RaidBossId = row[enemySchemaIndexes["RaidBossId"]].GetUInt32(),
                    Scale = row[enemySchemaIndexes["Scale"]].GetUInt16(),
                    Lv = row[enemySchemaIndexes["Lv"]].GetUInt16(),
                    HmPresetNo = row[enemySchemaIndexes["HmPresetNo"]].GetUInt16(),
                    StartThinkTblNo = row[enemySchemaIndexes["StartThinkTblNo"]].GetByte(),
                    RepopNum = row[enemySchemaIndexes["RepopNum"]].GetByte(),
                    RepopCount = row[enemySchemaIndexes["RepopCount"]].GetByte(),
                    EnemyTargetTypesId = row[enemySchemaIndexes["EnemyTargetTypesId"]].GetByte(),
                    MontageFixNo = row[enemySchemaIndexes["MontageFixNo"]].GetByte(),
                    SetType = row[enemySchemaIndexes["SetType"]].GetByte(),
                    InfectionType = row[enemySchemaIndexes["InfectionType"]].GetByte(),
                    IsBossGauge = row[enemySchemaIndexes["IsBossGauge"]].GetBoolean(),
                    IsBossBGM = row[enemySchemaIndexes["IsBossBGM"]].GetBoolean(),
                    IsManualSet = row[enemySchemaIndexes["IsManualSet"]].GetBoolean(),
                    IsAreaBoss = row[enemySchemaIndexes["IsAreaBoss"]].GetBoolean(),
                    BloodOrbs = row[enemySchemaIndexes["BloodOrbs"]].GetUInt32(),
                    HighOrbs = row[enemySchemaIndexes["HighOrbs"]].GetUInt32(),
                    Experience = row[enemySchemaIndexes["Experience"]].GetUInt32(),
                };
                int dropsTableId = row[enemySchemaIndexes["DropsTableId"]].GetInt32();
                if(dropsTableId >= 0)
                {
                    enemy.DropsTable = asset.DropsTables[(uint) dropsTableId];
                }
                enemies.Add(enemy);
                asset.Enemies[(layoutId, subGroupId)] = enemies;
            }

            return asset;
        }

        protected uint ParseHexUInt(string str)
        {
            str = str.TrimStart('0', 'x');
            return uint.Parse(str, NumberStyles.HexNumber, null);
        }

        private Dictionary<string, int> findSchemaIndexes(string[] reference, List<string> schema)
        {
            Dictionary<string, int> schemaIndexes = new Dictionary<string, int>();
            foreach (string header in reference)
            {
                int index = schema.IndexOf(header);
                if (index != -1)
                {
                    schemaIndexes.Add(header, index);
                }
            }
            return schemaIndexes;
        }
    }
}