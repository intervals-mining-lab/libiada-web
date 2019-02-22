namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;

    using LibiadaWeb.Models.CalculatorsData;

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
        /// Fills group and sequence type params in matter.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        public static void FillGroupAndSequenceType(Matter matter)
        {
            string name = matter.Name.ToLower();
            switch (matter.Nature)
            {
                case Nature.Literature:
                    matter.Group = Group.ClassicalLiterature;
                    matter.SequenceType = SequenceType.CompleteText;
                    break;
                case Nature.Music:
                    matter.Group = Group.ClassicalMusic;
                    matter.SequenceType = SequenceType.CompleteMusicalComposition;
                    break;
                case Nature.MeasurementData:
                    matter.Group = Group.ObservationData;
                    matter.SequenceType = SequenceType.CompleteNumericSequence;
                    break;
                case Nature.Genetic:
                    if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
                    {
                        matter.Group = Group.Eucariote;
                        matter.SequenceType = name.Contains("16s") ? SequenceType.Mitochondrion16SRRNA
                                            : name.Contains("plasmid") ? SequenceType.MitochondrialPlasmid
                                            : SequenceType.MitochondrionGenome;
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
                    else if (name.Contains("plastid") || name.Contains("apicoplast"))
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
                        matter.Group = name.Contains("virus") || name.Contains("viroid") || name.Contains("virophage") ?
                                           Group.Virus : Group.Bacteria;
                        matter.SequenceType = SequenceType.CompleteGenome;
                    }

                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(matter.Nature), (int)matter.Nature, typeof(Nature));
            }
        }

        /// <summary>
        /// The create matter.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence.
        /// </param>
        public void CreateMatterFromSequence(CommonSequence commonSequence)
        {
            Matter matter = commonSequence.Matter;
            if (matter != null)
            {
                matter.Sequence = new Collection<CommonSequence>();
                commonSequence.MatterId = CreateMatter(matter);
            }
            else
            {
                commonSequence.Matter = db.Matter.Single(m => m.Id == commonSequence.MatterId);
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
        /// Get matter select list.
        /// </summary>
        /// <param name="matters">
        /// The matters.
        /// </param>
        /// <param name="selected">
        /// The selected.
        /// </param>
        /// <returns>
        /// The <see cref="T:IEnumerable{MattersTableRow}"/>.
        /// </returns>
        public IEnumerable<MattersTableRow> GetMatterSelectList(IEnumerable<Matter> matters, Func<Matter, bool> selected)
        {
            return matters.OrderBy(m => m.Created).Select(m => new MattersTableRow(m, selected(m)));
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
                                 Name = $"{ExtractMatterName(metadata)} | {metadata.Version.CompoundAccession}",
                                 Nature = Nature.Genetic
                             };

            FillGroupAndSequenceType(matter);

            return matter;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
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
        private static string ExtractMatterName(GenBankMetadata metadata)
        {
            string species = metadata.Source.Organism.Species.GetLargestRepeatingSubstring();
            string commonName = metadata.Source.CommonName;
            string definition = metadata.Definition.TrimEnd(", complete genome.")
                                                   .TrimEnd(", complete sequence.")
                                                   .TrimEnd(", complete CDS.")
                                                   .TrimEnd(", complete cds.")
                                                   .TrimEnd(", genome.");

            if (commonName.Contains(species) || species.IsSubsetOf(commonName))
            {
                if (definition.Contains(commonName) || commonName.IsSubsetOf(definition))
                {
                    return definition;
                }

                if (commonName.Contains(definition) || definition.IsSubsetOf(commonName))
                {
                    return commonName;
                }

                return $"{commonName} | {definition}";
            }

            if (species.Contains(commonName) || commonName.IsSubsetOf(species))
            {
                if (definition.Contains(species) || species.IsSubsetOf(definition))
                {
                    return definition;
                }

                if (species.Contains(definition) || definition.IsSubsetOf(species))
                {
                    return species;
                }

                return $"{species} | {definition}";
            }

            throw new Exception($"Sequences names are not equal. CommonName = {commonName }, Species = {species}, Definition = {definition}");
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
        private IEnumerable<object> GetMatterSelectList(IEnumerable<Matter> matters)
        {
            return matters.OrderBy(m => m.Created).Select(m => new
            {
                Value = m.Id,
                Text = m.Name,
                Selected = false,
                SequenceType = m.SequenceType.GetDisplayValue(),
                Group = m.Group.GetDisplayValue(),
                m.Nature
            });
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
            if (matter.Nature == Nature.Music)
            {
                var dbMatter = db.Matter.Where(m => m.Name == matter.Name).ToList();
                if (dbMatter.Count > 0)
                {
                    return dbMatter.First().Id;
                }
            }
            db.Matter.Add(matter);
            db.SaveChanges();
            return matter.Id;
        }
    }
}
