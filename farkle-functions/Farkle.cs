using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Azure;
using Azure.Data.Tables;
using Azure.Identity;

namespace farkle_functions
{
    public class Farkle
    {
        private readonly ILogger<Farkle> _logger;
        private readonly string _uriString = "http://127.0.0.1:10002/devstoreaccount1/farkle?sv=2018-03-28&spr=https%2Chttp&st=2024-05-29T02%3A51%3A08Z&se=2024-05-30T02%3A51%3A08Z&sp=ra&sig=XHE3WBYT1CL6Mq%2F6RhiOOlUUeqAwdL12mw70XEOb7%2Bc%3D&tn=farkle";
        private readonly string _signature = "?sv=2018-03-28&spr=https%2Chttp&st=2024-05-29T02%3A51%3A08Z&se=2024-05-30T02%3A51%3A08Z&sp=ra&sig=XHE3WBYT1CL6Mq%2F6RhiOOlUUeqAwdL12mw70XEOb7%2Bc%3D&tn=farkle";
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public Farkle(ILogger<Farkle> logger)
        {
            _logger = logger;
            _tableServiceClient = new TableServiceClient(
                new Uri(_uriString), 
                new AzureSasCredential(_signature));
            _tableClient = _tableServiceClient.GetTableClient("farkle");
        }

        [Function("NewGame")]
        public IActionResult NewGame([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "NewGame/{numberOfPlayers}")] HttpRequest req
            , int numberOfPlayers)
        {
            _logger.LogInformation("NewGame triggered via http.");

            if (numberOfPlayers == 0) return new BadRequestResult();

            GameStartedFact gameStartedFact = new GameStartedFact()
                { NumberOfPlayers = numberOfPlayers };


            string serializedFact = JsonConvert.SerializeObject(gameStartedFact);
            string guidString = Guid.NewGuid().ToString();
            string modifiedGuidString = Regex.Replace(guidString, @"[^0-9]", "");
            BigInteger keyBigInteger = BigInteger.Parse(modifiedGuidString, NumberStyles.AllowHexSpecifier);
            FarkleFactEntity factEntity = new FarkleFactEntity
            {
                PartitionKey = "farkleTest",
                RowKey = keyBigInteger.ToString(),
                SerializedFact = serializedFact,
                ItemCreateDate = DateTime.Now.ToUniversalTime()
            };

            _tableClient.AddEntity(factEntity);

            return new JsonResult(gameStartedFact);
        }

        [Function("TryLuck")]
        public IActionResult TryLuck([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TryLuck/{diceRolled}/{meldKept}/{turnEnded}")] HttpRequest req
            , string diceRolled, string meldKept, bool turnEnded)
        {
            _logger.LogInformation("TryLuck triggered via http.");

            LuckTriedFact luckTriedFact =
                new LuckTriedFact
                {
                    DiceRolled = (diceRolled ?? "").Split(',').Select(int.Parse).ToArray(),
                    MeldKept = (meldKept ?? "").Split(',').Select(int.Parse).ToArray(),
                    TurnEnded = turnEnded
                };

            string serializedFact = JsonConvert.SerializeObject(luckTriedFact);
            string guidString = Guid.NewGuid().ToString();
            string modifiedGuidString = Regex.Replace(guidString, @"[^0-9]", "");
            BigInteger keyBigInteger = BigInteger.Parse(modifiedGuidString, NumberStyles.AllowHexSpecifier);
            FarkleFactEntity factEntity = new FarkleFactEntity
            {
                PartitionKey = "farkleTest",
                RowKey = keyBigInteger.ToString(),
                SerializedFact = serializedFact,
                ItemCreateDate = DateTime.Now.ToUniversalTime()
            };
            _tableClient.AddEntity(factEntity);

            RollGeneratedFact rollGeneratedFact = new RollGeneratedFact();
            List<int> newDiceValues = [];
            Random rnd = new Random();
            for (int i = 0; i < luckTriedFact.DiceRolled.Length; i++)
            {
                newDiceValues.Add(rnd.Next(1, 7));
            }
            rollGeneratedFact.DiceValues = newDiceValues.ToArray();
            string serializedFact2 = JsonConvert.SerializeObject(rollGeneratedFact);

            string guidString2 = Guid.NewGuid().ToString();
            string modifiedGuidString2 = Regex.Replace(guidString2, @"[^0-9]", "");
            BigInteger keyBigInteger2 = BigInteger.Parse(modifiedGuidString2, NumberStyles.AllowHexSpecifier);
            FarkleFactEntity factEntity2 = new FarkleFactEntity
            {
                PartitionKey = "farkleTest",
                RowKey = keyBigInteger2.ToString(),
                SerializedFact = serializedFact2,
                ItemCreateDate = DateTime.Now.ToUniversalTime()
            };

            _tableClient.AddEntity(factEntity2);
           
            //Store Roll Generated HERE

            return new JsonResult(rollGeneratedFact);
        }

        [Function("GetFacts")]
        public IActionResult GetFacts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetFacts")] HttpRequest req)
        {
            _logger.LogInformation("GetFacts triggered via http.");

            Pageable<FarkleFactEntity> farkleFactEntities = _tableClient.Query<FarkleFactEntity>();

            FarkleFactEntity[] factEntities = farkleFactEntities.ToArray();

            return new JsonResult(factEntities);
        }
    }
}
