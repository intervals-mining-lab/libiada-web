﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;

    using LibiadaWeb.Tasks;

    using Newtonsoft.Json;

    using SequenceGenerator;

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class SequencesOrderDistributionController : AbstractResultController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesOrderDistributionController"/> class.
        /// </summary>
        public SequencesOrderDistributionController() : base(TaskType.SequencesOrderDistribution)
        {
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.data = "{}";
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="alphabetCardinality">
        /// The alphabet cardinality.
        /// </param>
        /// <param name="generateStrict">
        /// The generate strict.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(int length, int alphabetCardinality, int typeGenerate)
        {
            return CreateTask(() =>
            {
                ISequenceGenerator sequenceGenerator = null;
                var orderGenerator = new OrderGenerator();
                var orders = new List<int[]>();
                switch (typeGenerate)
                {
                    case 0:
                        sequenceGenerator = new StrictSequenceGenerator();
                        orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                        break;
                    case 1:
                        sequenceGenerator = new SequenceGenerator();
                        orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                        break;
                    case 2:
                        sequenceGenerator = new NonRedundantStrictSequenceGenerator();
                        orders = orderGenerator.StrictGenerateOrders(length, alphabetCardinality);
                        break;
                    case 3:
                        sequenceGenerator = new NonRedundantSequenceGenerator();
                        orders = orderGenerator.GenerateOrders(length, alphabetCardinality);
                        break;
                    default: throw new ArgumentException("Invalid type of generate");
                }
                var sequences = sequenceGenerator.GenerateSequences(length, alphabetCardinality);
                var SequecesOrdersDistribution = new Dictionary<int[], List<BaseChain>>(new OrderEqualityComparer());
                foreach (var order in orders)
                {
                    SequecesOrdersDistribution.Add(order, new List<BaseChain>());
                }

                foreach (var sequence in sequences)
                {
                    SequecesOrdersDistribution[sequence.Building].Add(sequence);
                }

                var result = new Dictionary<string, object>
                {
                    { 
                        "result", SequecesOrdersDistribution.Select(r => new
                        {
                            order = r.Key,
                            sequences = r.Value.Select(s => s.ToString(",")).ToArray()
                        }) 
                    }
                };

                return new Dictionary<string, string> { { "data", JsonConvert.SerializeObject(result) } };
            });
        }
    }
}