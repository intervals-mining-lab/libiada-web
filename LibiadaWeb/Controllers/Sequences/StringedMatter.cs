namespace LibiadaWeb.Controllers.Sequences
{
        /// <summary>
        /// Stringed matter for drop down lists ids.
        /// </summary>
        public struct StringedMatter
        {
            /// <summary>
            /// The id.
            /// </summary>
            public readonly long Id;

            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The description.
            /// </summary>
            public readonly string Description;

            /// <summary>
            /// The nature.
            /// </summary>
            public readonly string Nature;

            /// <summary>
            /// The group.
            /// </summary>
            public readonly string Group;

            /// <summary>
            /// The sequence type.
            /// </summary>
            public readonly string SequenceType;

            /// <summary>
            /// The matters sequences count.
            /// </summary>
            public readonly int SequencesCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="StringedMatter"/> struct
            /// from given matter instance.
            /// </summary>
            /// <param name="matter">
            /// The matter.
            /// </param>
            /// <param name="sequencesCount">
            /// Matter's sequences Count.
            /// </param>
            public StringedMatter(Matter matter, int sequencesCount)
            {
                Id = matter.Id;
                Name = matter.Name;
                Description = matter.Description;
                Nature = ((byte)matter.Nature).ToString();
                Group = ((byte)matter.Group).ToString();
                SequenceType = ((byte)matter.SequenceType).ToString();
                SequencesCount = sequencesCount;
            }
        }
    }
