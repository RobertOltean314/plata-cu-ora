using Google.Cloud.Firestore;

namespace PlataCuOraApp.Server.Domain.DTOs
{
    [FirestoreData]
    public class OrarUserDTO
    {
        [FirestoreProperty]
        public int NrPost { get; set; }

        [FirestoreProperty]
        public string DenPost { get; set; } = string.Empty;

        [FirestoreProperty]
        public int OreCurs { get; set; }

        [FirestoreProperty]
        public int OreSem { get; set; }

        [FirestoreProperty]
        public int OreLab { get; set; }

        [FirestoreProperty]
        public int OreProi { get; set; }

        [FirestoreProperty]
        public string Tip { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Formatia { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Ziua { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ImparPar { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Materia { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SaptamanaInceput { get; set; } = string.Empty;

        [FirestoreProperty]
        public int TotalOre { get; set; }

        public override bool Equals(object obj)
        {
            return obj is OrarUserDTO other &&
                   NrPost == other.NrPost &&
                   DenPost == other.DenPost &&
                   OreCurs == other.OreCurs &&
                   OreSem == other.OreSem &&
                   OreLab == other.OreLab &&
                   OreProi == other.OreProi &&
                   Tip == other.Tip &&
                   Formatia == other.Formatia &&
                   Ziua == other.Ziua &&
                   ImparPar == other.ImparPar &&
                   Materia == other.Materia &&
                   SaptamanaInceput == other.SaptamanaInceput &&
                   TotalOre == other.TotalOre;
        }
    }
}
