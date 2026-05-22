namespace RealFenixFailures.Domain.Entities;

public class FenixFailureSystem {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;

    public ICollection<FenixFailureGroup> FailureGroups { get; set; } = new List<FenixFailureGroup>();
}
