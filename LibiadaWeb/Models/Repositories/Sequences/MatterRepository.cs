namespace LibiadaWeb.Models.Repositories.Sequences
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using Bio.IO.GenBank;

    using LibiadaCore.Extensions;
    using LibiadaWeb.Models.Repositories.Catalogs;

    /// <summary>
    /// The matter repository.
    /// </summary>
    public class MatterRepository : IMatterRepository
    {
        /// <summary>
        /// GenBank date formats.
        /// </summary>
        private readonly string[] GenBankDateFormats = new[] { "dd-MMM-yyyy", "MMM-yyyy", "yyyy", "yyyy-MM-ddTHH:mmZ", "yyyy-MM-ddTHHZ", "yyyy-MM-dd", "yyyy-MM" };

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
                case Nature.Image:
                    // TODO: add distinction between photo and picture, painting and photo
                    matter.Group = Group.Picture;
                    matter.SequenceType = SequenceType.CompleteImage;
                    break;

                case Nature.Genetic:
                    if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
                    {
                        matter.Group = Group.Eucariote;
                        matter.SequenceType = name.Contains("16s") ? SequenceType.Mitochondrion16SRRNA
                                            : name.Contains("plasmid") ? SequenceType.MitochondrialPlasmid
                                            : SequenceType.MitochondrialGenome;
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
                        matter.Group = name.Contains("virus") || name.Contains("viroid") || name.Contains("phage") ?
                                           Group.Virus : Group.Bacteria;
                        matter.SequenceType = SequenceType.CompleteGenome;
                    }

                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(matter.Nature), (int)matter.Nature, typeof(Nature));
            }
        }

        /// <summary>
        /// Creates new matter or extracts existing matter from database.
        /// </summary>
        /// <param name="commonSequence">
        /// The common sequence to be used for matter creation or extraction.
        /// </param>
        public void CreateOrExtractExistingMatterForSequence(CommonSequence commonSequence)
        {
            Matter matter = commonSequence.Matter;
            if (matter != null)
            {
                matter.Sequence = new Collection<CommonSequence>();
                commonSequence.MatterId = SaveToDatabase(matter);
            }
            else
            {
                commonSequence.Matter = db.Matter.Single(m => m.Id == commonSequence.MatterId);
            }
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
            var sources = metadata.Features.All.Where(f => f.Key == "source").ToArray();
            string collectionCountry = SequenceAttributeRepository.GetAttributeSingleValue(sources, "country");
            
            string collectionDateValue = SequenceAttributeRepository.GetAttributeSingleValue(sources, "collection_date").Split('/')[0];
            bool hasCollectionDate = DateTime.TryParseExact(collectionDateValue, GenBankDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime collectionDate);
            if(!string.IsNullOrEmpty(collectionDateValue) && !hasCollectionDate)
            {
                throw new Exception($"Collection date was invalid. Value: {collectionDateValue}.");
            }
            
            var matter = new Matter
            {
                Name = $"{ExtractMatterName(metadata)} | {metadata.Version.CompoundAccession}",
                Nature = Nature.Genetic,
                CollectionCountry = collectionCountry,
                CollectionDate = hasCollectionDate ? (DateTime?)collectionDate : null
            };

            FillGroupAndSequenceType(matter);

            return matter;
        }

        /// <summary>
        /// Adds given matter to database.
        /// </summary>
        /// <param name="matter">
        /// The matter.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public long SaveToDatabase(Matter matter)
        {
            db.Matter.Add(matter);
            db.SaveChanges();
            Cache.Clear();
            return matter.Id;
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

            throw new Exception($"Sequences names are not equal. CommonName = {commonName}, Species = {species}, Definition = {definition}");
        }
    }
}
