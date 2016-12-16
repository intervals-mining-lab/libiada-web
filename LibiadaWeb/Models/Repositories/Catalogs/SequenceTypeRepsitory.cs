namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class SequenceTypeRepsitory
    {
        public static SequenceType ExtractSequenceGroup(string name)
        {
            name = name.ToLower();
            if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
            {
                if (name.Contains("16s"))
                {
                    return SequenceType.Mitochondrion16SRRNA;
                }
                else
                {
                    return SequenceType.MitochondrionGenome;
                }
            }
            else if (name.Contains("chloroplast"))
            {
                return SequenceType.ChloroplastGenome;
            }
            else if (name.Contains("plasmid"))
            {
                return SequenceType.Plasmid;
            }
            else if (name.Contains("plastid"))
            {
                return SequenceType.Plastid;
            }
            else if (name.Contains("16s"))
            {
                return SequenceType.RRNA16S;
            }
            else if (name.Contains("18s"))
            {
                return SequenceType.RRNA18S;
            }
            else
            {
                return SequenceType.CompleteGenome;
            }
        }
    }
}