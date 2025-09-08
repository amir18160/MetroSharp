namespace Application.Dashboard.Queries.GetStats
{
    public class StatsDto
    {
        public int TotalDocumentsInDatabase { get; set; }
        public int TotalDocumentsAddedLast24Hours { get; set; }
        public int TotalDocumentsAddedLastWeek { get; set; }
        public int TotalDocumentsAddedLastMonth { get; set; }
        
        public int TotalUniqueMoviesInDatabase { get; set; }
        public int TotalUniqueSeriesInDatabase { get; set; }
        
        public int CurrentTaskInProcess { get; set; }
        public int CurrentTaskInQueue { get; set; }
        
        public int TotalFailedTasks { get; set; }
        public int TotalFailedTasksLast24Hours { get; set; }
        public int TotalSuccessfulTasks { get; set; }
        public int TotalSuccessfulTasksLast24Hours { get; set; }
        
    }
}