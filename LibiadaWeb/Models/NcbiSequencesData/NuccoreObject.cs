namespace LibiadaWeb.Models.NcbiSequencesData
{
    /// <summary>
    /// Contans search search results for one sequence from GenBank.
    /// </summary>
    public struct NuccoreObject
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public string Length { get; set; }

        public string Accession { get; set; }

        public override bool Equals(object obj) =>  obj is NuccoreObject other && other.Accession == Accession;

        public override int GetHashCode() => Accession.GetHashCode();

        public static bool operator ==(NuccoreObject left, NuccoreObject right) => left.Equals(right);

        public static bool operator !=(NuccoreObject left, NuccoreObject right) => !(left == right);
    }
}
