////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTracker_API\Data\DataContext.cs </file>
///
/// <copyright file="DataContext.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the data context class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using BugTracker_API.Models;

namespace BugTracker_API.Data
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A data context. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class DataContext : DbContext
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="options">  Options for controlling the operation. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the projects. </summary>
        ///
        /// <value> The projects. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DbSet<Project> Projects { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the issues. </summary>
        ///
        /// <value> The issues. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DbSet<Issue> Issues { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the comments. </summary>
        ///
        /// <value> The comments. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DbSet<Comment> Comments { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the users. </summary>
        ///
        /// <value> The users. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DbSet<User> Users { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from
        /// the entity types exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties
        /// on your derived context. The resulting model may be cached and re-used for subsequent
        /// instances of your derived context.
        /// </summary>
        ///
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via
        /// <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        ///
        /// <param name="modelBuilder"> The builder being used to construct the model for this context.
        ///                             Databases (and other extensions) typically define extension
        ///                             methods on this object that allow you to configure aspects of the
        ///                             model that are specific to a given database. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make Issue soft delete
            modelBuilder.Entity<Issue>().Property<bool>("isDeleted");
            modelBuilder.Entity<Issue>().HasQueryFilter(m => EF.Property<bool>(m, "isDeleted") == false);

            modelBuilder.Entity<Project>()
                .HasMany(project => project.Issues)
                .WithOne(issue => issue.Project)
                .IsRequired();

            modelBuilder.Entity<Issue>()
                .HasMany(i => i.Comments)
                .WithOne(c => c.Issue)
                .IsRequired();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <para>
        ///                 Saves all changes made in this context to the database.
        ///             </para>
        /// <para>
        ///                 This method will automatically call
        /// <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to
        /// discover any changes to entity instances before saving to the underlying database. This can
        /// be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        ///             </para>
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   The number of state entries written to the database. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override int SaveChanges()
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <para>
        ///                 Saves all changes made in this context to the database.
        ///             </para>
        /// <para>
        ///                 This method will automatically call
        /// <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to
        /// discover any changes to entity instances before saving to the underlying database. This can
        /// be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        ///             </para>
        /// <para>
        ///                 Multiple active operations on the same context instance are not supported.
        ///                 Use 'await' to ensure that any asynchronous operations have completed before
        ///                 calling another method on this context.
        ///             </para>
        /// </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="acceptAllChangesOnSuccess">    Indicates whether
        ///                                             <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" />
        ///                                             is called after the changes have been sent
        ///                                             successfully to the database. </param>
        /// <param name="cancellationToken">            (Optional) A
        ///                                             <see cref="T:System.Threading.CancellationToken" />
        ///                                             to observe while waiting for the task to complete. </param>
        ///
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the number
        /// of state entries written to the database.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Updates the soft delete statuses. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void UpdateSoftDeleteStatuses()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if(entry.Entity.GetType() == typeof(Issue))
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.CurrentValues["isDeleted"] = false;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            entry.CurrentValues["isDeleted"] = true;
                            break;
                    }
                }
            }
        }
    }
}
