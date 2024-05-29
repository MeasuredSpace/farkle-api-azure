

using Azure;
using Azure.Data.Tables;


namespace farkle_functions;

public class FarkleFactEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string SerializedFact { get; set; }
    public DateTime? ItemCreateDate { get; set; }
}

public class GameStartedFact
{
    public int NumberOfPlayers { get; set; }
}

public class LuckTriedFact
{
    public int[] DiceRolled { get; set; }
    public int[] MeldKept { get; set; }
    public bool TurnEnded { get; set; }
}

public class RollGeneratedFact
{
    public int[] DiceValues { get; set; }
}

public class GameEndedFact
{
    public string Winner { get; set; }    
    public int FinalScore { get; set; }
}
