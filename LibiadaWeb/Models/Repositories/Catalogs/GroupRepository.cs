namespace LibiadaWeb.Models.Repositories.Catalogs
{
    public class GroupRepository
    {
        public static Group ExtractSequenceGroup(string name)
        {
            name = name.ToLower();
            if (name.Contains("mitochondrion") || name.Contains("mitochondrial") || name.Contains("18s"))
            {
                return Group.Eucariote;
            }
            else if (name.Contains("virus"))
            {
                return Group.Virus;
            }
            else
            {
                return Group.Bacteria;
            }
        }
    }
}