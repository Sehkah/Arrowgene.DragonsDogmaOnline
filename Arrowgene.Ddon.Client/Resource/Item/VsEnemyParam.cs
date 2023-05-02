using System;
using Arrowgene.Buffers;

namespace Arrowgene.Ddon.Client.Resource.Item;

public class VsEnemyParam
{
    public enum EM_PHYLOGENY_KIND
    {
        EM_PHYLOGENY_KIND_NONE = 0x0,
        EM_PHYLOGENY_KIND_DEMI_HUMAN = 0x1,
        EM_PHYLOGENY_KIND_BEAST = 0x2,
        EM_PHYLOGENY_KIND_KIZIN = 0x3,
        EM_PHYLOGENY_KIND_ZOMBIE = 0x4,
        EM_PHYLOGENY_KIND_SKELETON = 0x5,
        EM_PHYLOGENY_KIND_WINGED = 0x6,
        EM_PHYLOGENY_KIND_GIANT = 0x7,
        EM_PHYLOGENY_KIND_SOFT_BODY = 0x8,
        EM_PHYLOGENY_KIND_GHOST = 0x9,
        EM_PHYLOGENY_KIND_CURSE = 0xA,
        EM_PHYLOGENY_KIND_ART_EVIL = 0xB,
        EM_PHYLOGENY_KIND_HUMAN = 0xC,
        EM_PHYLOGENY_KIND_ALCHEMY = 0xD,
        EM_PHYLOGENY_KIND_DRAGON = 0xE,
        EM_PHYLOGENY_KIND_EVIL = 0xF,
        EM_PHYLOGENY_KIND_EROSION = 0x10
    }

    public byte KindType { get; set; }
    public string KindTypeName { get; set; }
    public ushort Param { get; set; }

    public static VsEnemyParam ReadVsEnemyParam(IBuffer buffer)
    {
        var vsEnemyParam = new VsEnemyParam();
        vsEnemyParam.KindType = buffer.ReadByte();
        if (vsEnemyParam.KindType > (int)EM_PHYLOGENY_KIND.EM_PHYLOGENY_KIND_EROSION)
            throw new Exception($"Versus Enemy Type can not be bigger than maximum expected {(int)EM_PHYLOGENY_KIND.EM_PHYLOGENY_KIND_EROSION}!");
        vsEnemyParam.KindTypeName = ((Param.PARAM_KIND)vsEnemyParam.KindType).ToString();

        vsEnemyParam.Param = buffer.ReadUInt16();
        return vsEnemyParam;
    }
}
