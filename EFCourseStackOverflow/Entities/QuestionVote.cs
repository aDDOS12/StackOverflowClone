namespace EFCourseStackOverflow.Entities
{
    public class QuestionVote : Vote
    {
        //foreign key
        public int QuestionId { get; set; }
        //navigation property
        public Question Question { get; set; } = default!;

    }
}
