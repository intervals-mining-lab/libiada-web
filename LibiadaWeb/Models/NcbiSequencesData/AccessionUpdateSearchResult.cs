namespace LibiadaWeb.Models.NcbiSequencesData
{
    using System;

    public class AccessionUpdateSearchResult
    {
        public string LocalAccession;
        public string Name;
        public string RemoteName;
        public string RemoteOrganism;
        public byte LocalVersion;
        public byte RemoteVersion;
        public string LocalUpdateDate;
        public DateTimeOffset LocalUpdateDateTime;
        public string RemoteUpdateDate;
        public bool Updated;
        public bool NameUpdated;
    }
}