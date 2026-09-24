namespace StackOverflowClone.Api.Entities;

public class Tag
{
    public int Id { get; set; }
    public string TagName { get; set; } = default!;
    public List<Question> Questions { get; set; } = new List<Question>();
}
