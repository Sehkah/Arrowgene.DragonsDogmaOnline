using System;
using System.Collections.Generic;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Job.AbilityList
{
    namespace Arrowgene.Ddon.Client.Resource.Job
    {
        /**
     * rAbilityList : cResource
     */
        public class AbilityList : ResourceFile
        {
            public AbilityList()
            {
                DataList = new List<AbilityData>();
            }

            /**
             * MtTypedArray
             * <cAbilityData> : MtArray
             */
            public List<AbilityData> DataList { get; set; }

            public uint Version { get; set; }
            public uint Count { get; set; }

            // TODO Unknown additional data needs to be understood, but otherwise the basic parsing logic works
            protected override void ReadResource(IBuffer buffer)
            {
                Version = ReadUInt32(buffer);
                var UnknownByte1 = ReadByte(buffer);
                var UnknownByte2 = ReadByte(buffer);

                var UnknownShort1 = ReadUInt16(buffer);
                Count = ReadUInt32(buffer);

                var UnknownInt1 = ReadUInt32(buffer);

                if (Version > 8)
                {
                    var UnknownByte3 = ReadByte(buffer);
                }

                DataList.Clear();

                for (var i = 0; i <= Count - 2; i++) DataList.Add(ReadAbilityData(buffer));
            }

            public AbilityData ReadAbilityData(IBuffer buffer)
            {
                var abilityData = new AbilityData();
                abilityData.ParamArray = new List<AbilityParam>();
                abilityData.ParamArray.Clear();
                abilityData.ParamArray.AddRange(ReadMtArray(buffer, ReadAbilityParam));


                if (Version > 8)
                {
                    abilityData.ParamArrayV9 = new List<AbilityParamV9>();
                    var additionalDataAvailable = ReadByte(buffer);
                    if (additionalDataAvailable != 0)
                    {
                        var abilityParamV9 = new AbilityParamV9();
                        abilityParamV9.UnknownParam1 = ReadInt32(buffer);
                        abilityParamV9.UnknownParam2 = ReadInt32(buffer);
                        abilityData.ParamArrayV9.Add(abilityParamV9);
                        Logger.Debug($"Additional data finished reading at position {buffer.Position}: {abilityParamV9}");
                    }
                }

                return abilityData;
            }

            public AbilityParam ReadAbilityParam(IBuffer buffer)
            {
                var abilityParam = new AbilityParam();

                abilityParam.ParamType = ReadInt32(buffer);
                if (abilityParam.ParamType is < 0 or > 57) throw new Exception($"ParamType must be within range 0-57, found {abilityParam.ParamType}!");

                abilityParam.ParamTypeName = ((AbilityParamTypeEnum)abilityParam.ParamType).ToString();

                abilityParam.CorrectType = ReadInt32(buffer);
                if (abilityParam.CorrectType is < 0 or > 1) throw new Exception($"CorrectType must be within range 0-1, found {abilityParam.CorrectType}!");

                abilityParam.CorrectTypeName = ((AbilityParamCorrectTypeEnum)abilityParam.CorrectType).ToString();

                abilityParam.ParamDataArray = new List<ParamData>();
                abilityParam.ParamDataArray.Clear();
                abilityParam.ParamDataArray.AddRange(ReadMtArray(buffer, ReadParamData));

                return abilityParam;
            }

            public ParamData ReadParamData(IBuffer buffer)
            {
                var paramData = new ParamData();
                paramData.Lv = ReadInt32(buffer);
                paramData.Param = ReadInt32(buffer);
                return paramData;
            }

            /**
         * cAbilityData : MtObject
         */
            public class AbilityData
            {
                public List<AbilityParam> ParamArray { get; set; }
                public List<AbilityParamV9> ParamArrayV9 { get; set; }
            }

            /**
             * MtTypedArray
             * <cAbilityParam> : MtArray
             */
            public class AbilityParam
            {
                public int ParamType { get; set; }

                // Custom attribute for user friendly name resolution
                public string ParamTypeName { get; set; }

                public int CorrectType { get; set; }

                // Custom attribute for user friendly name resolution
                public string CorrectTypeName { get; set; }
                public List<ParamData> ParamDataArray { get; set; }
            }

            /**
         * cAbilityParam::cParamData : MtObject
         */
            public class ParamData
            {
                public int Lv { get; set; }
                public int Param { get; set; }
            }

            public class AbilityParamV9
            {
                public int UnknownParam1 { get; set; }
                public int UnknownParam2 { get; set; }
            }
        }

        internal enum AbilityParamTypeEnum
        {
            PARAM_TYPE_NONE = 0x0,
            PARAM_TYPE_WEIGHT = 0x1,
            PARAM_TYPE_HP = 0x2,
            PARAM_TYPE_STAMINA = 0x3,
            PARAM_TYPE_LOST = 0x4,
            PARAM_TYPE_ATTACK = 0x5,
            PARAM_TYPE_DEFENCE = 0x6,
            PARAM_TYPE_M_ATTACK = 0x7,
            PARAM_TYPE_M_DEFENCE = 0x8,
            PARAM_TYPE_MUSCLE = 0x9,
            PARAM_TYPE_PIYO = 0xA,
            PARAM_TYPE_STRENGTH = 0xB,
            PARAM_TYPE_GUTS = 0xC,
            PARAM_TYPE_DEF_FIRE = 0xD,
            PARAM_TYPE_DEF_ICE = 0xE,
            PARAM_TYPE_DEF_THUNDER = 0xF,
            PARAM_TYPE_DEF_HORY = 0x10,
            PARAM_TYPE_DEF_DARK = 0x11,
            PARAM_TYPE_REG_FIRE = 0x12,
            PARAM_TYPE_REG_ICE = 0x13,
            PARAM_TYPE_REG_THUNDER = 0x14,
            PARAM_TYPE_REG_HORY = 0x15,
            PARAM_TYPE_REG_DARK = 0x16,
            PARAM_TYPE_REG_POISON = 0x17,
            PARAM_TYPE_REG_SLOW = 0x18,
            PARAM_TYPE_REG_SLEEP = 0x19,
            PARAM_TYPE_REG_PIYO = 0x1A,
            PARAM_TYPE_REG_WATER = 0x1B,
            PARAM_TYPE_REG_OIL = 0x1C,
            PARAM_TYPE_REG_SEAL = 0x1D,
            PARAM_TYPE_REG_CURSE = 0x1E,
            PARAM_TYPE_REG_SOFT = 0x1F,
            PARAM_TYPE_REG_PETRI = 0x20,
            PARAM_TYPE_REG_GOLD = 0x21,
            PARAM_TYPE_DEF_LOW_FIRE = 0x22,
            PARAM_TYPE_DEF_LOW_ICE = 0x23,
            PARAM_TYPE_DEF_LOW_THUNDER = 0x24,
            PARAM_TYPE_DEF_LOW_HOLY = 0x25,
            PARAM_TYPE_DEF_LOW_DARK = 0x26,
            PARAM_TYPE_DEF_LOW_ATK = 0x27,
            PARAM_TYPE_DEF_LOW_DEF = 0x28,
            PARAM_TYPE_DEF_LOW_M_ATK = 0x29,
            PARAM_TYPE_DEF_LOW_M_DEF = 0x2A,
            PARAM_TYPE_SPECIAL = 0x2B,
            PARAM_TYPE_DOWN_POWER = 0x2C,
            PARAM_TYPE_DAMAGE_UP = 0x2D,
            PARAM_TYPE_STUN_UP = 0x2E,
            PARAM_TYPE_OCD_UP = 0x2F,
            PARAM_TYPE_BLOW_UP = 0x30,
            PARAM_TYPE_CHANCE_UP = 0x31,
            PARAM_TYPE_TIRED_UP = 0x32,
            PARAM_TYPE_HEAL_HP_UP = 0x33,
            PARAM_TYPE_HEAL_STAMINA_UP = 0x34,
            PARAM_TYPE_DEF_EROSION = 0x35,
            PARAM_TYPE_DEF_ITEM_SEAL = 0x36,
            PARAM_TYPE_UNKNOWN1 = 0x37,
            PARAM_TYPE_UNKNOWN2 = 0x38,
            PARAM_TYPE_UNKNOWN3 = 0x39
        }

        internal enum AbilityParamCorrectTypeEnum
        {
            CORRECT_TYPE_ADD = 0x0,
            CORRECT_TYPE_RATE = 0x1
        }
    }
}
