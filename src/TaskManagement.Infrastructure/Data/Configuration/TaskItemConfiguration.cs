using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Configuration for the TaskItem entity in the database
    /// </summary>
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        /// <summary>
        /// Configures the TaskItem entity
        /// </summary>
        /// <param name="builder">Entity type builder</param>
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .UseIdentityColumn()
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.AssignedTo)
                .HasMaxLength(100)
                .IsRequired(false);
        }
    }
} 