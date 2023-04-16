namespace Arrowgene.Ddon.Client.Resource.Item;

public class VsEnemyParam
{
    public enum EM_PHYLOGENY_KIND
    {
        EM_PHYLOGENY_KIND_NONE = 0x0,
        EM_PHYLOGENY_KIND_01 = 0x1,
        EM_PHYLOGENY_KIND_02 = 0x2,
        EM_PHYLOGENY_KIND_03 = 0x3,
        EM_PHYLOGENY_KIND_04 = 0x4,
        EM_PHYLOGENY_KIND_05 = 0x5,
        EM_PHYLOGENY_KIND_06 = 0x6,
        EM_PHYLOGENY_KIND_07 = 0x7,
        EM_PHYLOGENY_KIND_08 = 0x8,
        EM_PHYLOGENY_KIND_09 = 0x9,
        EM_PHYLOGENY_KIND_0A = 0xA,
        EM_PHYLOGENY_KIND_0B = 0xB,
        EM_PHYLOGENY_KIND_0C = 0xC,
        EM_PHYLOGENY_KIND_0D = 0xD,
        EM_PHYLOGENY_KIND_0E = 0xE,
        EM_PHYLOGENY_KIND_0F = 0xF,
        EM_PHYLOGENY_KIND_10 = 0x10,
        EM_PHYLOGENY_KIND_NUM = 0x11
    }

    public byte KindType { get; set; }
    public ushort Param { get; set; }
}
