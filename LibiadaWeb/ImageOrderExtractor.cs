using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibiadaWeb
{
    public enum ImageOrderExtractor : byte
    {
        /// <summary>
        /// Reads image line by line from top to bottom and from left to right.
        /// </summary>
        [Display(Name = "Read image left to right top to bottom")]
        [Description("Reads image line by line from top to bottom and from left to right")]
        LineLeftToRightTopToBottom = 1,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineLeftToRightBottomToTop = 2,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineRightToLeftTopToBottom = 3,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineRightToLeftBottomToTop = 4,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineZigzagLeftCornerTopToBottom = 5,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineZigzagLeftCornerBottomToTop = 6,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineZigzagRightCornerTopToBottom = 7,

        /// <summary>
        /// .
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        LineZigzagRightCornerBottomToTop = 8,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        ColumnTopToBottomLeftToRight = 9,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        ColumnTopToBottomRightToLeft = 10,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        ColumnBottomToTopLeftToRight = 11,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        ColumnBottomToTopRightToLeft = 12,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromCenterClockwise = 13,
        
        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromCenterCounterClockwise = 14,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromLeftTopCornerClockwise = 15,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromRightTopCornerClockwise = 16,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromLeftBottomCornerClockwise = 17,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromRightBottomCornerClockwise = 18,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromLeftTopCornerCounterClockwise = 19,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromRightTopCornerCounterClockwise = 20,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromLeftBottomCornerCounterClockwise = 21,

        /// <summary>
        ///.
        /// </summary>
        [Display(Name = "")]
        [Description("")]
        SpiralFromRightBottomCornerCounterClockwise = 22,
    }
}