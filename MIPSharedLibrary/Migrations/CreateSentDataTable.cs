using FluentMigrator;

namespace MIPSharedLibrary.Migrations
{
    [Migration(20240412)] // Use a unique timestamp or version number for the migration
    public class CreateSentDataTable : Migration
    {
        public override void Up()
        {
            Create.Table("SentData")
                .WithColumn("CorrelationId").AsGuid().PrimaryKey()          // Unique identifier
                .WithColumn("Data").AsString(int.MaxValue).NotNullable()    // JSON payload (large string)
                .WithColumn("SentDate").AsDateTime().NotNullable()          // Timestamp of when the data was sent
                .WithColumn("DataInfo").AsInt32().NotNullable()             // Enum stored as integer
                .WithColumn("IsSuccessful").AsBoolean().NotNullable()       // Boolean success flag
                .WithColumn("Response").AsString(int.MaxValue).Nullable();  // JSON response (optional)
        }

        public override void Down()
        {
            Delete.Table("SentData");
        }
    }
}