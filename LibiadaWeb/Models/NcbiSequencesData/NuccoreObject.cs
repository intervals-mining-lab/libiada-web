namespace LibiadaWeb.Models.NcbiSequencesData
{
    using System;

    /// <summary>
    /// Contans search search results for one sequence from GenBank.
    /// </summary>
    public struct NuccoreObject
    {
        public string Title { get; set; }

        public DateTimeOffset UpdateDate { get; set; }

        public string AccessionVersion { get; set; }

        public string Completeness { get; set; }

        public string Organism { get; set; }

        public override bool Equals(object other) =>  other is NuccoreObject nuccoreObject && nuccoreObject.AccessionVersion == AccessionVersion;

        public override int GetHashCode() => AccessionVersion.GetHashCode();

        public static bool operator ==(NuccoreObject left, NuccoreObject right) => left.Equals(right);

        public static bool operator !=(NuccoreObject left, NuccoreObject right) => !(left == right);
    }
}
