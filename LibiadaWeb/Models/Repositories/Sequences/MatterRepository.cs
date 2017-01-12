namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

    /// <summary>
    /// The matter repository.
    /// </summary>
    public class MatterRepository : IMatterRepository
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatterRepository"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public MatterRepository(LibiadaWebEntities db)
        {
            this.db = db;
        }

        /// <summary>
        /// The create matter.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        public void CreateMatterFromSequence(CommonSequence commonSequence)
        {
            var matter = commonSequence.Matter;
            if (matter != null)
            {
                matter.Sequence = new Collection<CommonSequence>();
                commonSequence.MatterId = CreateMatter(matter);
            }
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList()
        {
            return GetMatterSelectList(m => true);
        }

        /// <summary>
        /// The get select list with nature.
        /// </summary>
        /// <param name="matters">
        /// The matters.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList(IEnumerable<Matter> matters)
        {
            return matters.OrderBy(m => m.Created).Select(m => new
            {
                Value = m.Id,
                Text = m.Name,
                Selected = false,
                Created = m.Created.ToString(),
                Modified = m.Modified.ToString(),
                SequenceType = m.SequenceType.GetDisplayValue(),
                Group = m.Group.GetDisplayValue(),
                m.Nature,
                m.Description
            });
        }

        /// <summary>
        /// The get matter select list.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        public IEnumerable<object> GetMatterSelectList(Func<Matter, bool> filter)
        {
            return GetMatterSelectList(db.Matter.Where(filter));
        }

        /// <summary>
        /// Creates matter from genBank metadata.
        /// </summary>
        /// <param name="metadata">
        /// The metadata.
        /// </param>
        /// <returns>
        /// The <see cref="Matter"/>.
        /// </returns>
        public Matter CreateMatterFromGenBankMetadata(GenBankMetadata metadata)
        {
            var matter = new Matter
                             {
                                 Name = ExtractMatterName(metadata) + " | " + metadata.Version.CompoundAccession,
                                 Nature = Nature.Genetic
                             };

            FillGropAndSequenceType(matter);

            return matter;
        }

        /// <summary>
        /// Extracts supposed sequence name from metadata.
        /// </summary>
        /// <param name="metadata">
        /// The metadata.
        /// </param>
        /// <returns>
        /// Supposed name as <see cref="string"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if all name fields are contradictory.
        /// </exception>
        public string ExtractMatterName(GenBankMetadata metadata)
        {
            string species = metadata.Source.Organism.Species.GetLargestRepeatingSubstring();
            string commonName = metadata.Source.CommonName;
            string definition = metadata.Definition.TrimEnd(", complete genome.")
                                                   .TrimEnd(", complete sequence.")
                                                   .TrimEnd(", complete CDS.")
                                                   .TrimEnd(", complete cds.")
                                                   .TrimEnd(", genome.");

            if (commonName.Contains(species))
            {
                if (definition.Contains(commonName))
                {
                    return definition;
                }

                if (commonName.Contains(definition))
                {
                    return commonName;
                }

                return commonName + " | " + definition;
            }

            if (species.Contains(commonName))
            {
                if (definition.Contains(species))
                {
                    return definition;
                }

                if (species.Contains(definition))
                {
                    return species;
                }

                return species + " | " + definition;
            }

            throw new Exception("Sequences names are not equal. CommonName = " + commonName +
                                ", Species = " + species +
                                ", Definition = " + definition);
        }

        /// <summary>
        /// Fills grop and sequence type params i matter.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        public void FillGropAndSequenceType(Matter matter)
        {
            var name = matter.Name.ToLower();
            if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
            {
                matter.Group = Group.Eucariote;
                matter.SequenceType = name.Contains("16s") ? SequenceType.Mitochondrion16SRRNA : SequenceType.MitochondrionGenome;
            }
            else if (name.Contains("18s"))
            {
                matter.Group = Group.Eucariote;
                matter.SequenceType = SequenceType.RRNA18S;
            }
            else if (name.Contains("chloroplast"))
            {
                matter.Group = Group.Eucariote;
                matter.SequenceType = SequenceType.ChloroplastGenome;
            }
            else if (name.Contains("plastid"))
            {
                matter.Group = Group.Eucariote;
                matter.SequenceType = SequenceType.Plastid;
            }
            else if (name.Contains("plasmid"))
            {
                matter.Group = Group.Bacteria;
                matter.SequenceType = SequenceType.Plasmid;
            }
            else if (name.Contains("16s"))
            {
                matter.Group = Group.Bacteria;
                matter.SequenceType = SequenceType.RRNA16S;
            }
            else
            {
                matter.Group = name.Contains("virus") ? Group.Virus : Group.Bacteria;
                matter.SequenceType = SequenceType.CompleteGenome;
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// The create matter.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        private long CreateMatter(Matter matter)
        {
            db.Matter.Add(matter);
            db.SaveChanges();
            return matter.Id;
        }
    }
}
