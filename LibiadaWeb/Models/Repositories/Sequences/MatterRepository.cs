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
        /// Determines group and sequence type params for matter.
        /// </summary>
        /// <param name="name">
        /// The matter's name.
        /// </param>
        /// <param name="nature">
        /// Nature of the matter.
        /// </param>
        /// <returns>
        /// Tuple of <see cref="Group"/> and <see cref="SequenceType"/>.
        /// </returns>
        public static (Group, SequenceType) GetGroupAndSequenceType(string name, Nature nature)
        {
            name = name.ToLower();
            switch (nature)
            {
                case Nature.Literature:
                    return (Group.ClassicalLiterature, SequenceType.CompleteText);
                case Nature.Music:
                    return (Group.ClassicalMusic, SequenceType.CompleteMusicalComposition);
                case Nature.MeasurementData:
                    return (Group.ObservationData, SequenceType.CompleteNumericSequence);
                case Nature.Image:
                    // TODO: add distinction between photo and picture, painting and photo
                    return (Group.Picture, SequenceType.CompleteImage);
                case Nature.Genetic:
                    if (name.Contains("mitochondrion") || name.Contains("mitochondrial"))
                    {
                        SequenceType sequenceType = name.Contains("16s") ? SequenceType.Mitochondrion16SRRNA
                                                  : name.Contains("plasmid") ? SequenceType.MitochondrialPlasmid
                                                  : SequenceType.MitochondrialGenome;
                        return (Group.Eucariote, sequenceType);
                    }
                    else if (name.Contains("18s"))
                    {
                        return (Group.Eucariote, SequenceType.RRNA18S);
                    }
                    else if (name.Contains("chloroplast"))
                    {
                        return (Group.Eucariote, SequenceType.ChloroplastGenome);
                    }
                    else if (name.Contains("plastid") || name.Contains("apicoplast"))
                    {
                        return (Group.Eucariote, SequenceType.Plastid);
                    }
                    else if (name.Contains("plasmid"))
                    {
                        return (Group.Bacteria, SequenceType.Plasmid);
                    }
                    else if (name.Contains("16s"))
                    {
                        return (Group.Bacteria, SequenceType.RRNA16S);
                    }
                    else
                    {
                        Group group = name.Contains("virus") || name.Contains("viroid") || name.Contains("phage") ? Group.Virus
                                    : name.Contains("archaea") ? Group.Archaea
                                                               : Group.Bacteria;
                        return (group, SequenceType.CompleteGenome);
                    }
                default:
                    throw new InvalidEnumArgumentException(nameof(nature), (int)nature, typeof(Nature));
            }
        }

        /// <summary>
        /// Trims the name ending of the GenBank sequence.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <returns>
        /// Trimmed name as <see cref="string"/>
        /// </returns>
        public static string TrimGenBankNameEnding(string name)
        {
            return name.TrimEnd('.')
                       .TrimEnd(", complete genome")
                       .TrimEnd(", complete sequence")
                       .TrimEnd(", complete CDS")
                       .TrimEnd(", complete cds")
                       .TrimEnd(", genome");
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
            string collectionCoordinates = SequenceAttributeRepository.GetAttributeSingleValue(sources, "lat_lon");

            string collectionDateValue = SequenceAttributeRepository.GetAttributeSingleValue(sources, "collection_date")?.Split('/')[0];
            bool hasCollectionDate = DateTime.TryParseExact(collectionDateValue, GenBankDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime collectionDate);
            if (!string.IsNullOrEmpty(collectionDateValue) && !hasCollectionDate)
            {
                throw new Exception($"Collection date was invalid. Value: {collectionDateValue}.");
            }

            string species = metadata.Source.Organism.Species;
            string commonName = metadata.Source.CommonName;
            string definition = metadata.Definition;

            var matter = new Matter
            {
                Name = $"{ExtractMatterName(species, commonName, definition)} | {metadata.Version.CompoundAccession}",
                Nature = Nature.Genetic,
                CollectionCountry = collectionCountry,
                CollectionLocation = collectionCoordinates,
                CollectionDate = hasCollectionDate ? (DateTime?)collectionDate : null
            };

            (matter.Group, matter.SequenceType) = GetGroupAndSequenceType($"{species} {commonName} {definition}", matter.Nature);

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
        /// Removes sequence type from the the sequence name.
        /// </summary>
        /// <param name="name">The GenBank sequence name.</param>
        /// <returns>
        /// Cleaned up name as <see cref="string"/>.
        /// </returns>
        private static string RemoveSequenceTypeFromName(string name)
        {
            return name.Replace("mitochondrion", "")
                       .Replace("plastid", "")
                       .Replace("plasmid", "")
                       .Replace("chloroplast", "")
                       .Replace("  ", " ")
                       .Trim();
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
        private static string ExtractMatterName(string species, string commonName, string definition)
        {
            species = RemoveSequenceTypeFromName(species.GetLargestRepeatingSubstring());
            commonName = RemoveSequenceTypeFromName(commonName);
            definition = RemoveSequenceTypeFromName(TrimGenBankNameEnding(definition));

            if (commonName.Contains(definition) || definition.IsSubsetOf(commonName))
            {
                if (species.Contains(commonName) || commonName.IsSubsetOf(species))
                {
                    return species;
                }

                if (commonName.Contains(species) || species.IsSubsetOf(commonName))
                {
                    return commonName;
                }

                return $"{commonName} | {species}";
            }

            if (definition.Contains(commonName) || commonName.IsSubsetOf(definition))
            {
                if (species.Contains(definition) || definition.IsSubsetOf(species))
                {
                    return species;
                }

                if (definition.Contains(species) || species.IsSubsetOf(definition))
                {
                    return definition;
                }

                return $"{species} | {definition}";
            }

            if (commonName.Contains(species) || species.IsSubsetOf(commonName))
            {
                return $"{commonName} | {definition}";
            }

            if (species.Contains(commonName) || commonName.IsSubsetOf(species))
            {
                return $"{species} | {definition}";
            }

            if (species.Contains(definition) || definition.IsSubsetOf(species))
            {
                return $"{commonName} | {species}";
            }

            throw new Exception($"Sequences names are not equal. CommonName = {commonName}, Species = {species}, Definition = {definition}");
        }
    }
}
