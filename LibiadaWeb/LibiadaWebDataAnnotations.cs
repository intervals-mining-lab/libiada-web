using System.ComponentModel.DataAnnotations;

namespace LibiadaWeb
{
    [MetadataType(typeof(CharacteristicApplicabilityDataAnnotations))]
    public partial class characteristic_applicability
    {
    }

    public class CharacteristicApplicabilityDataAnnotations
    {
        [Display(Name = "Название")]
        public string name { get; set; }

        [Display(Name = "Описание")]
        public string description { get; set; }
    }
}