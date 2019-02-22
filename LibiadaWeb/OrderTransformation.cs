using LibiadaCore.Core;
using LibiadaWeb.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LibiadaWeb
{
    public enum OrderTransformation: byte
    {
        [Display(Name = "Dissimilar order")]
        [Link(Link.NotApplied)]
        Dissimilar = 1,

        [Display(Name = "High order with link to the begining")]
        [Link(Link.Start)]
        HighOrderToStart = 2,

        [Display(Name = "High order with link to the end")]
        [Link(Link.End)]
        HighOrderToEnd = 3,

        [Display(Name = "High order with cyclic link to the begining")]
        [Link(Link.CycleStart)]
        HighOrderCyclicToStart = 4,

        [Display(Name = "High order with cyclic link to the end")]
        [Link(Link.CycleEnd)]
        HighOrderCyclicToEnd = 5
    }
}