using Arrowgene.Ddon.GameServer.Characters;
using Arrowgene.Ddon.GameServer.Chat;
using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Shared.Entity;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Ddon.Shared.Model.Rpc;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arrowgene.Ddon.GameServer
{
    public class RpcManager
    {
        private class RpcTrackingMap : Dictionary<uint, RpcCharacterData>
        {
            public DateTime TimeStamp { get; set; }

            public RpcTrackingMap() : base() 
            { 
                TimeStamp = DateTime.Now;
            }

            public bool Update(DateTime newTimestamp, List<RpcCharacterData> characterData)
            {
                if (newTimestamp <= TimeStamp) return false;

                TimeStamp = newTimestamp;
                this.Clear();
                foreach (var character in characterData)
                {
                    this[character.CharacterId] = character;
                }
                return true;
            }
        }


        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(RpcManager));

        private static readonly string[] TRAFFIC_LABELS = new string[] {
            "Empty", "Light", "Good", "Normal", "Busy", "Heavy"
        };
        private static readonly uint COUNT_PER_TRAFFIC = 10;

        private readonly HttpClient HttpClient = new HttpClient();

        private readonly DdonGameServer Server;
        private readonly Dictionary<ushort, ServerInfo> ChannelInfo;

        private readonly Dictionary<ushort, RpcTrackingMap> CharacterTrackingMap;

        public class RpcWrappedObject
        {
            public RpcInternalCommand Command { get; set; }
            public ushort Origin { get; set; }
            public object Data { get; set; }
            public DateTime Timestamp { get; set; }
            public RpcWrappedObject()
            {
                Timestamp = DateTime.UtcNow;
            }
        }

        public class RpcUnwrappedObject
        {
            public RpcInternalCommand Command { get; set; }
            public ushort Origin { get; set; }
            public DateTime Timestamp { get; set; }

            [JsonConverter(typeof(DataJsonConverter))]
            public string Data { get; set; }
            public T GetData<T>()
            {
                return JsonSerializer.Deserialize<T>(Data);
            }

            // Hack to deserialize nested objects.
            internal class DataJsonConverter : JsonConverter<string>
            {
                public override string Read(
                    ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                    {
                        return jsonDoc.RootElement.GetRawText();
                    }
                }

                public override void Write(
                    Utf8JsonWriter writer, string value, JsonSerializerOptions options)
                {
                    throw new NotImplementedException();
                }
            }
        }


        public RpcManager(DdonGameServer server)
        {
            Server = server;
            ChannelInfo = Server.AssetRepository.ServerList.ToDictionary(x => x.Id,
                x => new ServerInfo()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Brief = x.Brief,
                    MaxLoginNum = x.MaxLoginNum,
                    LoginNum = x.LoginNum,
                    Addr = x.Addr,
                    Port = x.Port,
                    IsHide = x.IsHide,
                    RpcPort = x.RpcPort,
                    RpcAuthToken = x.RpcAuthToken,
                });
            CharacterTrackingMap = new();
            foreach (var info in ChannelInfo.Values)
            {
                CharacterTrackingMap[info.Id] = new();
            }

            string authToken = string.Empty;
            if (ChannelInfo.ContainsKey((ushort)Server.Id))
            {
                authToken = ChannelInfo[(ushort)Server.Id].RpcAuthToken;
            }

            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Internal", $"{server.Id}:{authToken}");
        }

        #region Server List
        public List<CDataGameServerListInfo> ServerListInfo()
        {
            return ChannelInfo.Keys.Select(x => ServerListInfo(x)).ToList();
        }

        public CDataGameServerListInfo ServerListInfo(ushort channelId)
        {
            var info = ChannelInfo[channelId].ToCDataGameServerListInfo();
            if (channelId == Server.Id)
            {
                // Check against StageId to not count clients that are in the character select.
                info.LoginNum = (uint)Server.ClientLookup.GetAll().Where(x => x.Character != null && x.Character.Stage.Id != 0).Count();
            }
            else
            {
                lock (CharacterTrackingMap[channelId])
                {
                    info.LoginNum = (uint)CharacterTrackingMap[channelId].Count;
                }
            }
            
            info.TrafficName = GetTrafficName(info.LoginNum);
            return info;
        }

        private static string GetTrafficName(uint count)
        {
            uint index = 0;
            if (count > 0)
            {
                index = count / COUNT_PER_TRAFFIC + 1;
                index = (uint) Math.Min(index, TRAFFIC_LABELS.Length);
            }
            return $"{TRAFFIC_LABELS[index]} ({count})";
        }
        #endregion

        #region RPC Machinery
        public bool Auth(ushort channelId, string token)
        {
            return ChannelInfo.Values.Where(x => x.Id == channelId && x.RpcAuthToken == token).Any();
        }

        private string Route(ushort channelId, string route)
        {
            var channel = ChannelInfo[channelId];
            return $"http://{channel.Addr}:{channel.RpcPort}/rpc/{route}";
        }

        public async Task<T> Post<T>(ushort channelId, string route, RpcInternalCommand command, object data)
        {
            var response = await HttpClient.PostAsJsonAsync(Route(channelId, route), new RpcWrappedObject()
            {
                Command = command,
                Origin = (ushort) Server.Id,
                Data = data
            });

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public void Announce(ushort channelId, string route, RpcInternalCommand command, object data)
        {
            var wrappedObject = new RpcWrappedObject()
            {
                Command = command,
                Origin = (ushort)Server.Id,
                Data = data
            };

            var json = JsonSerializer.Serialize(wrappedObject);

            _ = HttpClient.PostAsync(Route(channelId, route), new StringContent(json));
        }

        public void AnnounceAll(string route, RpcInternalCommand command, object data)
        {
            foreach (var id in ChannelInfo.Keys)
            {
                Announce(id, route, command, data);
            }
        }

        public void AnnounceOthers(string route, RpcInternalCommand command, object data)
        {
            foreach (var id in ChannelInfo.Keys)
            {
                if (id == Server.Id) continue;
                Announce(id, route, command, data);
            }
        }

        public void AnnounceClan(uint clanId, string route, RpcInternalCommand command, object data)
        {
            foreach (var channel in CharacterTrackingMap)
            {
                if (channel.Key == Server.Id)
                {
                    continue;
                }

                if (channel.Value.Any(x => x.Value.ClanId == clanId))
                {
                    Announce(channel.Key, route, command, data);
                }
            }
        }
        #endregion

        #region Player Tracking
        public ushort FindPlayerByName(string firstName, string lastName)
        {
            foreach ((ushort channelId, var channelMembers) in CharacterTrackingMap)
            {
                lock(channelMembers)
                {
                    foreach (var player in channelMembers.Values)
                    {
                        if (player.FirstName == firstName && player.LastName == lastName)
                        {
                            return channelId;
                        }
                    }
                }
            }
            return 0;
        }

        public ushort FindPlayerById(uint characterId)
        {
            foreach ((ushort channelId, var channelMembers) in CharacterTrackingMap)
            {
                lock (channelMembers)
                {
                    if (channelMembers.ContainsKey(characterId))
                    {
                        return channelId;
                    }
                }
            }
            return 0;
        }
    
        public void AnnouncePlayerList()
        {
            List<RpcCharacterData> rpcCharacterDatas = new List<RpcCharacterData>();
            foreach (var character in Server.ClientLookup.GetAllCharacter().Where(x => x.Stage.Id != 0))
                {
                    rpcCharacterDatas.Add(new(character));
                }
            
            AnnounceOthers("internal/tracking", RpcInternalCommand.NotifyPlayerList, rpcCharacterDatas);
            CharacterTrackingMap[(ushort) Server.Id].Update(DateTime.Now, rpcCharacterDatas);
        }

        public void ReceivePlayerList(ushort channelId, DateTime timestamp, List<RpcCharacterData> characterDatas)
        {
            if (CharacterTrackingMap.ContainsKey(channelId))
            {
                if (!CharacterTrackingMap[channelId].Update(timestamp, characterDatas))
                {
                    Logger.Info($"Out of date character list discarded for channel ID {channelId}");
                }
            }
        }

        public void UpdatePlayerSummaryClan(uint characterId, uint clanId)
        {
            var clan = Server.ClanManager.GetClan(clanId);
            foreach ((ushort channelId, var channelMembers) in CharacterTrackingMap)
            {
                lock (channelMembers)
                {
                    if (channelMembers.ContainsKey(characterId))
                    {
                        channelMembers[characterId].ClanId = clan.ClanServerParam.ID;
                        channelMembers[characterId].ClanName = clan.ClanUserParam.Name;
                        channelMembers[characterId].ClanShortName = clan.ClanUserParam.ShortName;
                    }
                }
            }
        }
        #endregion

        #region Chat
        public void AnnounceClanChat(GameClient client, ChatResponse chatResponse)
        {
            if (client.Character.ClanId == 0) return;

            RpcChatData chatData = new RpcChatData()
            {
                HandleId = 0,
                Type = LobbyChatMsgType.Clan,
                MessageFlavor = chatResponse.MessageFlavor,
                PhrasesCategory = chatResponse.PhrasesCategory,
                PhrasesIndex = chatResponse.PhrasesIndex,
                Message = chatResponse.Message,
                Deliver = false,
                SourceData = new RpcCharacterData(client.Character)
            };

            AnnounceClan(client.Character.ClanId, "internal/chat", RpcInternalCommand.SendClanMessage, chatData);
        }

        public void AnnounceTellChat(GameClient client, C2SChatSendTellMsgReq request)
        {
            var targetServer = FindPlayerByName(request.CharacterInfo.CharacterName.FirstName, 
                request.CharacterInfo.CharacterName.LastName);

            if (targetServer == 0) throw new ResponseErrorException(ErrorCode.ERROR_CODE_CHAT_TELL_CHARACTER_OFFLINE);
            if (targetServer == Server.Id) throw new ResponseErrorException(ErrorCode.ERROR_CODE_CHAT_TELL_SESSION_LOST);

            RpcChatData chatData = new RpcChatData()
            {
                HandleId = 0,
                Type = LobbyChatMsgType.Tell,
                MessageFlavor = request.MessageFlavor,
                PhrasesCategory = request.PhrasesCategory,
                PhrasesIndex = request.PhrasesIndex,
                Message = request.Message,
                Deliver = false,
                SourceData = new RpcCharacterData(client.Character),
                TargetData = new RpcCharacterData()
                {
                    FirstName = request.CharacterInfo.CharacterName.FirstName,
                    LastName = request.CharacterInfo.CharacterName.LastName,
                    CharacterId = request.CharacterInfo.CharacterId
                }
            };

            Announce(targetServer, "internal/chat", RpcInternalCommand.SendTellMessage, chatData);
        }
        #endregion

        public void AnnounceAllPacket<T>(T packet, uint characterId = 0)
            where T : class, IPacketStructure, new()
        {
            RpcPacketData data = new RpcPacketData()
            {
                GroupId = packet.Id.GroupId,
                HandlerId = packet.Id.HandlerId,
                HandlerSubId = packet.Id.HandlerSubId,
                CharacterId = characterId,
                Data = EntitySerializer.Get<T>().Write(packet)
            };
            AnnounceAll("internal/packet", RpcInternalCommand.AnnouncePacketAll, data);
        }

        public void AnnounceClanPacket<T>(uint clanId, T packet, uint characterId = 0)
            where T : class, IPacketStructure, new()
        {
            if (clanId == 0) return;

            RpcPacketData data = new RpcPacketData()
            {
                GroupId = packet.Id.GroupId,
                HandlerId = packet.Id.HandlerId,
                HandlerSubId = packet.Id.HandlerSubId,
                ClanId = clanId,
                CharacterId = characterId,
                Data = EntitySerializer.Get<T>().Write(packet)
            };

            if (ClanManager.INTERNAL_IMPORTANT_PACKETS.Contains(packet.Id))
            {
                // This needs to be sent to all channels so they'll update their internal tracking of the clan, even if nobody is there to recieve the packet.
                AnnounceOthers("internal/packet", RpcInternalCommand.AnnouncePacketClan, data);
            }
            else
            {
                AnnounceClan(clanId, "internal/packet", RpcInternalCommand.AnnouncePacketClan, data);
            }
        }
    }
}