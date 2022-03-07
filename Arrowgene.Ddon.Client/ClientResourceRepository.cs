using System.Collections.Generic;
using System.IO;
using Arrowgene.Ddon.Client.Data;
using Arrowgene.Ddon.Client.Resource;
using Arrowgene.Ddon.Shared;
using Arrowgene.Logging;

namespace Arrowgene.Ddon.Client
{
    public class ClientResourceRepository
    {
        private static readonly ILogger Logger = LogProvider.Logger<Logger>(typeof(ClientResourceRepository));

        public FieldAreaList FieldAreaList { get; private set; }
        public AreaStageList AreaStageList { get; private set; }
        public AreaList AreaList { get; private set; }
        public StageList StageList { get; private set; }
        public LandListLal LandList { get; private set; }
        public StageToSpot StageToSpot { get; private set; }
        public EnemyGroup EnemyGroup { get; private set; }
        public Gmd FieldAreaNames { get; private set; }
        public Dictionary<uint, LocationData> StageLocations { get; private set; }
        public Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>> StageOmMarker { get; private set; }
        public Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>> StageSceMarker { get; private set; }
        public Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>> StageNpcMarker { get; private set; }
        public Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>> StageEctMarker { get; private set; }
        public Dictionary<uint, List<FieldAreaAdjoinList.AdjoinInfo>> StageAdJoin { get; private set; }
        public Dictionary<uint, List<StageToSpot.Entry>> StageSpots { get; private set; }
        public Dictionary<uint, List<EnemyGroup.Entry>> EnemyGroups { get; private set; }

        public ClientData Data { get; private set; }

        private DirectoryInfo _directory;

        public ClientResourceRepository()
        {
            // Nested
            Data = new ClientData();

            // Flat Lookup
            StageLocations = new Dictionary<uint, LocationData>();
            StageOmMarker = new Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>>();
            StageSceMarker = new Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>>();
            StageNpcMarker = new Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>>();
            StageEctMarker = new Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>>();
            StageAdJoin = new Dictionary<uint, List<FieldAreaAdjoinList.AdjoinInfo>>();
            StageSpots = new Dictionary<uint, List<StageToSpot.Entry>>();
            EnemyGroups = new Dictionary<uint, List<EnemyGroup.Entry>>();

            // Client Resources
            AreaStageList = new AreaStageList();
            AreaList = new AreaList();
            StageList = new StageList();
            FieldAreaList = new FieldAreaList();
            LandList = new LandListLal();
            StageToSpot = new StageToSpot();
            EnemyGroup = new EnemyGroup();
        }

        public void Load(DirectoryInfo romDirectory)
        {
            _directory = romDirectory;
            if (_directory == null || !_directory.Exists)
            {
                Logger.Error("Rom Path Invalid");
                return;
            }

            LandList = GetResource<LandListLal>("base.arc", "scr/land_list");
            AreaStageList = GetResource<AreaStageList>("base.arc", "scr/area_stage_list");
            AreaList = GetResource<AreaList>("base.arc", "scr/area_list");
            StageList = GetResource<StageList>("base.arc", "scr/stage_list");
            FieldAreaList = GetResource<FieldAreaList>("game_common.arc", "etc/FieldArea/field_area_list");
            StageToSpot = GetFile<StageToSpot>("game_common.arc", "param/stage_to_spot");
            EnemyGroup = GetFile<EnemyGroup>("game_common.arc", "param/enemy_group");
            FieldAreaNames = GetResource<Gmd>("game_common.arc", "ui/00_message/common/field_area_name", "gmd");

            foreach (StageList.Info sli in StageList.StageInfos)
            {
                LocationData lcd = GetResource_NoLog<LocationData>(
                    $"stage/st{sli.StageNo:0000}/st{sli.StageNo:0000}.arc",
                    $"scr/st{sli.StageNo:0000}/etc/st{sli.StageNo:0000}",
                    "lcd"
                );
                if (lcd != null)
                {
                    if (!StageLocations.ContainsKey(sli.StageNo))
                    {
                        StageLocations.Add(sli.StageNo, lcd);
                    }
                }
            }

            foreach (FieldAreaList.FieldAreaInfo fai in FieldAreaList.FieldAreaInfos)
            {
                Gmd.Entry areaName = FieldAreaNames.Entries[(int) fai.GmdIdx];
                FieldAreaMarkerInfo omMarker = GetResource_NoLog<FieldAreaMarkerInfo>(
                    $"/FieldArea/FieldArea{fai.FieldAreaId:000}_marker.arc",
                    $"etc/FieldArea/FieldArea{fai.FieldAreaId:000}_marker_om",
                    "fmi"
                );
                if (omMarker != null)
                {
                    AddMarker(omMarker.MarkerInfos, StageOmMarker);
                }

                FieldAreaMarkerInfo sceMarker = GetResource_NoLog<FieldAreaMarkerInfo>(
                    $"/FieldArea/FieldArea{fai.FieldAreaId:000}_marker.arc",
                    $"etc/FieldArea/FieldArea{fai.FieldAreaId:000}_marker_sce",
                    "fmi"
                );
                if (sceMarker != null)
                {
                    AddMarker(sceMarker.MarkerInfos, StageSceMarker);
                }

                FieldAreaMarkerInfo npcMarker = GetResource_NoLog<FieldAreaMarkerInfo>(
                    $"/FieldArea/FieldArea{fai.FieldAreaId:000}_marker.arc",
                    $"etc/FieldArea/FieldArea{fai.FieldAreaId:000}_marker_npc",
                    "fmi"
                );
                if (npcMarker != null)
                {
                    AddMarker(npcMarker.MarkerInfos, StageNpcMarker);
                }

                FieldAreaMarkerInfo ectMarker = GetResource_NoLog<FieldAreaMarkerInfo>(
                    $"/FieldArea/FieldArea{fai.FieldAreaId:000}_marker.arc",
                    $"etc/FieldArea/FieldArea{fai.FieldAreaId:000}_marker_ect",
                    "fmi"
                );
                if (ectMarker != null)
                {
                    AddMarker(ectMarker.MarkerInfos, StageEctMarker);
                }

                FieldAreaAdjoinList adjoin = GetResource_NoLog<FieldAreaAdjoinList>(
                    $"/FieldArea/FieldArea{fai.FieldAreaId:000}_marker.arc",
                    $"etc/FieldArea/FieldArea{fai.FieldAreaId:000}_adjoin",
                    "faa"
                );
                if (adjoin != null)
                {
                    AddAdjoin(adjoin.AdjoinInfos, StageAdJoin);
                }
            }

            foreach (StageToSpot.Entry sts in StageToSpot.Entries)
            {
                if (!StageSpots.ContainsKey(sts.StageNo))
                {
                    StageSpots[sts.StageNo] = new List<StageToSpot.Entry>();
                }

                StageSpots[sts.StageNo].Add(sts);
            }

            foreach (EnemyGroup.Entry emg in EnemyGroup.Entries)
            {
                if (!EnemyGroups.ContainsKey(emg.mEnemyGroupId))
                {
                    EnemyGroups[emg.mEnemyGroupId] = new List<EnemyGroup.Entry>();
                }

                EnemyGroups[emg.mEnemyGroupId].Add(emg);
            }

            // Land -> Area -> Stage -> Spot
            // for each land
            foreach (LandListLal.LandInfo land in LandList.LandInfos)
            {
                LandData landData = Data.ProvideLand(land.LandId);
                // for each area in the land
                foreach (uint areaId in land.AreaIds)
                {
                    AreaData areaData = Data.ProvideArea(areaId);
                    landData.AddArea(areaData);
                    foreach (AreaStageList.AreaInfoStage ais in AreaStageList.AreaInfoStages)
                    {
                        // find stages that belong to this area
                        if (ais.AreaId == areaId)
                        {
                            foreach (StageList.Info sli in StageList.StageInfos)
                            {
                                if (sli.StageNo == ais.StageNo)
                                {
                                    StageData stageData = Data.ProvideStage(sli.StageNo);
                                    stageData.AddOmMarker(StageOmMarker);
                                    stageData.AddEctMarker(StageEctMarker);
                                    stageData.AddNpcMarker(StageNpcMarker);
                                    stageData.AddSceMarker(StageSceMarker);
                                    stageData.AddAdJoin(StageAdJoin);
                                    stageData.AddLocations(StageLocations);
                                    stageData.AddSpots(StageSpots);

                                    areaData.AddStage(stageData);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddMarker(List<FieldAreaMarkerInfo.MarkerInfo> markers,
            Dictionary<uint, List<FieldAreaMarkerInfo.MarkerInfo>> dst)
        {
            foreach (FieldAreaMarkerInfo.MarkerInfo marker in markers)
            {
                if (!dst.ContainsKey((uint) marker.StageNo))
                {
                    dst[(uint) marker.StageNo] = new List<FieldAreaMarkerInfo.MarkerInfo>();
                }

                dst[(uint) marker.StageNo].Add(marker);
            }
        }

        private void AddAdjoin(List<FieldAreaAdjoinList.AdjoinInfo> adjoins,
            Dictionary<uint, List<FieldAreaAdjoinList.AdjoinInfo>> dst)
        {
            foreach (FieldAreaAdjoinList.AdjoinInfo adjoin in adjoins)
            {
                if (!dst.ContainsKey((uint) adjoin.DestinationStageNo))
                {
                    dst[(uint) adjoin.DestinationStageNo] = new List<FieldAreaAdjoinList.AdjoinInfo>();
                }

                dst[(uint) adjoin.DestinationStageNo].Add(adjoin);
            }
        }

        private T GetFile<T>(string arcPath, string filePath, string ext = null) where T : ClientFile, new()
        {
            ArcArchive.ArcFile arcFile = GetArcFile(arcPath, filePath, ext, true);
            if (arcFile == null)
            {
                return null;
            }

            T file = new T();
            file.Open(arcFile.Data);
            return file;
        }

        private T GetResource<T>(string arcPath, string filePath, string ext = null) where T : ResourceFile, new()
        {
            ArcArchive.ArcFile arcFile = GetArcFile(arcPath, filePath, ext, true);
            if (arcFile == null)
            {
                return null;
            }

            T resource = new T();
            resource.Open(arcFile.Data);
            return resource;
        }

        private T GetResource_NoLog<T>(string arcPath, string filePath, string ext = null) where T : ResourceFile, new()
        {
            ArcArchive.ArcFile arcFile = GetArcFile(arcPath, filePath, ext, false);
            if (arcFile == null)
            {
                return null;
            }

            T resource = new T();
            resource.Open(arcFile.Data);
            return resource;
        }

        private ArcArchive.ArcFile GetArcFile(string arcPath, string filePath, string ext, bool log)
        {
            string path = Path.Combine(_directory.FullName, Util.UnrootPath(arcPath));
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {
                if (log)
                {
                    Logger.Error($"File does not exist. ({path})");
                }

                return null;
            }

            ArcArchive archive = new ArcArchive();
            archive.Open(file.FullName);
            ArcArchive.FileIndexSearch search = ArcArchive.Search()
                .ByArcPath(filePath)
                .ByExtension(ext);
            ArcArchive.ArcFile arcFile = archive.GetFile(search);
            if (arcFile == null)
            {
                if (log)
                {
                    Logger.Error($"File:{filePath} could not be located in archive:{path}");
                }

                return null;
            }

            return arcFile;
        }
    }
}
