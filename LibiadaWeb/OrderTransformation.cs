using LibiadaCore.Core;
using System.ComponentModel.DataAnnotations;

namespace LibiadaWeb
{
    public enum OrderTransformation: byte
    {
        [Display(Name = "Dissimilar order")]
        [LibiadaCore.Attributes.Link(Link.NotApplied)]
        Dissimilar = 1,

        [Display(Name = "High order with link to the begining")]
        [LibiadaCore.Attributes.Link(Link.Start)]
        HighOrderToStart = 2,

        [Display(Name = "High order with link to the end")]
        [LibiadaCore.Attributes.Link(Link.End)]
        HighOrderToEnd = 3,

        [Display(Name = "High order with cyclic link to the begining")]
        [LibiadaCore.Attributes.Link(Link.CycleStart)]
        HighOrderCyclicToStart = 4,

        [Display(Name = "High order with cyclic link to the end")]
        [LibiadaCore.Attributes.Link(Link.CycleEnd)]
        HighOrderCyclicToEnd = 5
    }
}