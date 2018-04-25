namespace LibiadaWeb
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using LibiadaWeb.Attributes;

    public enum GroupType : short
    {
        [Display(Name = "Prose")]
        [Description("")]
        [Nature(Nature.Literature)]
        Prose = 1,

        [Display(Name = "Poems and poetry")]
        [Description("")]
        [Nature(Nature.Literature)]
        Poem = 2,

        [Display(Name = "Domain")]
        [Description("Domain")]
        [Nature(Nature.Genetic)]
        Domain = 3,

        [Display(Name = "Kingdom")]
        [Description("Kingdom")]
        [Nature(Nature.Genetic)]
        [GroupType(Domain)]
        Kingdom = 4,

        [Display(Name = "Phylum")]
        [Description("Phylum")]
        [Nature(Nature.Genetic)]
        [GroupType(Kingdom)]
        Phylum = 5
    }
}