using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Entities;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Entities.Configurations;
using AssetFlow.Data.Entities.Asset;
using AssetFlow.Data.Entities.SparePart;
using AssetFlow.Data.Entities.Maintenance;
using AssetFlow.Data.Entities.Issue;
using WorkOrderEntity = AssetFlow.Data.Entities.WorkOrder.WorkOrder;
using AssetFlow.Data.Entities.WorkOrder;
using AssetFlow.Data.Entities.Location;
using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Data.Extensions;
using AssetFlow.Data.Identity;
using AssetFlow.Data.Provider;
using AssetFlow.Data.Seeds;
using System.Linq.Expressions;


namespace AssetFlow.Data.Data
{
    public class ApplicationDbContext : ACLContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ITenantProvider _tenantProvider;
        // Expose tenant id as a property so EF Core query filters can reference
        // the DbContext instance and evaluate the tenant per DbContext/request.
        public int? TenantId => _tenantProvider?.GetTenantId();
        public ApplicationDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor, ITenantProvider tenantProvider) : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
            _tenantProvider = tenantProvider;
        }

        #region UserManagement
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        #endregion UserManagement

        #region Tenant
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<TenantLanguage> TenantLanguages { get; set; }
        public DbSet<TenantResource> TenantResources { get; set; }
        #endregion Tenant

        #region Location
        public DbSet<LocationType> LocationTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        #endregion Location

        #region Asset
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetComponent> AssetComponents { get; set; }
        #endregion Asset

        #region SparePart
        public DbSet<PartCategory> PartCategories { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PartSerialNumber> PartSerialNumbers { get; set; }
        public DbSet<PartInventory> PartInventories { get; set; }
        public DbSet<PartInventoryBatch> PartInventoryBatches { get; set; }
        public DbSet<PartTransaction> PartTransactions { get; set; }
        public DbSet<PartTransactionSerial> PartTransactionSerials { get; set; }
        public DbSet<PartReplacement> PartReplacements { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        #endregion SparePart

        #region Maintenance
        public DbSet<MaintenanceType> MaintenanceTypes { get; set; }
        public DbSet<MaintenanceChecklist> MaintenanceChecklists { get; set; }
        public DbSet<MaintenanceChecklistItem> MaintenanceChecklistItems { get; set; }
        public DbSet<MaintenanceChecklistItemOption> MaintenanceChecklistItemOptions { get; set; }
        public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public DbSet<PreventiveMaintenanceOccurrence> PreventiveMaintenanceOccurrences { get; set; }
        public DbSet<PreventiveMaintenanceOccurrenceChecklistItem> PreventiveMaintenanceOccurrenceChecklistItems { get; set; }
        public DbSet<PreventiveMaintenanceChecklistResponse> PreventiveMaintenanceChecklistResponses { get; set; }
        #endregion Maintenance

        #region Issue
        public DbSet<IssueCategory> IssueCategories { get; set; }
        public DbSet<AssetIssue> AssetIssues { get; set; }
        public DbSet<IssueAttachment> IssueAttachments { get; set; }
        #endregion Issue

        #region WorkOrder
        public DbSet<WorkOrderEntity> WorkOrders { get; set; }
        public DbSet<WorkOrderDiagnosis> WorkOrderDiagnoses { get; set; }
        public DbSet<WorkOrderStatusHistory> WorkOrderStatusHistories { get; set; }
        public DbSet<WorkOrderAssignmentHistory> WorkOrderAssignmentHistories { get; set; }
        public DbSet<WorkOrderAttachment> WorkOrderAttachments { get; set; }
        #endregion WorkOrder

        #region Configurations
        public DbSet<NavigationItemEnum> NavigationItemEnums { get; set; }
        public DbSet<NavigationCreateItemEnum> NavigationCreateItemEnums { get; set; }
        public DbSet<MenuItemEnum> MenuItemEnums { get; set; }
        public DbSet<MetaDataKeysEnum> MetaDataKeysEnums { get; set; }
        #endregion Configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Calling base for creating keys for identity tables
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("ApplicationUsers");
            modelBuilder.Entity<ApplicationRole>().ToTable("ApplicationRoles");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("ApplicationUserRoles");

            modelBuilder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new
                {
                    ur.UserId,
                    ur.RoleId
                });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

            });
            modelBuilder.Entity<ApplicationUser>(user =>
            {
                user.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(m => m.CreatedById);

                user.HasIndex(u => u.NormalizedUserName)
                    .HasDatabaseName("UserNameIndex")
                    .IsUnique(false);

                user.HasIndex(u => u.NormalizedEmail)
                    .HasDatabaseName("EmailIndex")
                    .IsUnique(false);

                user.HasIndex(u => new { u.TenantId, u.NormalizedUserName })
                    .IsUnique();

                user.HasIndex(u => new { u.TenantId, u.NormalizedEmail })
                    .IsUnique();
            });

            modelBuilder.Entity<ApplicationRole>(role =>
            {
                role.HasIndex(r => r.NormalizedName)
                    .HasDatabaseName("RoleNameIndex")
                    .IsUnique(false);

                role.HasIndex(r => new { r.TenantId, r.NormalizedName })
                    .IsUnique();
            });
            
            modelBuilder.Entity<Tenant>(tenant =>
            {
                tenant.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(m => m.CreatedById);

                tenant.HasOne(e => e.ModifiedBy)
                      .WithMany()
                      .HasForeignKey(m => m.ModifiedById);
            });

            modelBuilder.Entity<TenantResource>(tr =>
            {
                tr.HasKey(x => x.Id);
                tr.HasIndex(x => new { x.TenantId, x.ResourceId }).IsUnique();
                tr.HasOne(x => x.Tenant)
                    .WithMany(t => t.TenantResources)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
                tr.HasOne(x => x.Resource)
                    .WithMany()
                    .HasForeignKey(x => x.ResourceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LocationType>(lt =>
            {
                lt.HasOne(x => x.ParentLocationType)
                    .WithMany(x => x.ChildLocationTypes)
                    .HasForeignKey(x => x.ParentLocationTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Location>(loc =>
            {
                loc.HasOne(x => x.LocationType)
                    .WithMany(x => x.Locations)
                    .HasForeignKey(x => x.LocationTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                loc.HasOne(x => x.ParentLocation)
                    .WithMany(x => x.ChildLocations)
                    .HasForeignKey(x => x.ParentLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssetCategory>(ac =>
            {
                ac.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            });

            modelBuilder.Entity<AssetType>(at =>
            {
                at.HasIndex(x => new { x.TenantId, x.AssetCategoryId, x.Code }).IsUnique();
                at.HasOne(x => x.AssetCategory)
                    .WithMany(x => x.AssetTypes)
                    .HasForeignKey(x => x.AssetCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Asset>(a =>
            {
                a.HasIndex(x => new { x.TenantId, x.AssetCode }).IsUnique();
                a.HasOne(x => x.AssetCategory).WithMany().HasForeignKey(x => x.AssetCategoryId).OnDelete(DeleteBehavior.Restrict);
                a.HasOne(x => x.AssetType).WithMany().HasForeignKey(x => x.AssetTypeId).OnDelete(DeleteBehavior.Restrict);
                a.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
                a.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssetComponent>(c =>
            {
                c.HasIndex(x => new { x.TenantId, x.ComponentCode }).IsUnique();
                c.HasOne(x => x.Asset).WithMany(x => x.Components).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartCategory>(pc =>
            {
                pc.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            });

            modelBuilder.Entity<Part>(p =>
            {
                p.HasIndex(x => new { x.TenantId, x.PartNumber }).IsUnique();
                p.HasOne(x => x.PartCategory).WithMany(x => x.Parts).HasForeignKey(x => x.PartCategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Supplier>(s =>
            {
                s.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            });

            modelBuilder.Entity<PartSerialNumber>(psn =>
            {
                psn.HasIndex(x => new { x.TenantId, x.SerialNumber }).IsUnique();
                psn.Property(x => x.SupplierSerialReference).HasMaxLength(200).IsRequired();
                psn.HasOne(x => x.Part).WithMany(x => x.SerialNumbers).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                psn.HasOne(x => x.PartInventoryBatch).WithMany(x => x.SerialNumbers).HasForeignKey(x => x.PartInventoryBatchId).OnDelete(DeleteBehavior.Restrict);
                psn.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
                psn.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartInventory>(pi =>
            {
                pi.HasIndex(x => new { x.TenantId, x.PartId, x.LocationId }).IsUnique();
                pi.HasOne(x => x.Part).WithMany(x => x.InventoryItems).HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                pi.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartInventoryBatch>(pib =>
            {
                pib.HasIndex(x => new { x.TenantId, x.BatchReference }).IsUnique();
                pib.HasOne(x => x.PartInventory).WithMany(x => x.Batches).HasForeignKey(x => x.PartInventoryId).OnDelete(DeleteBehavior.Restrict);
                pib.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                pib.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartTransaction>(pt =>
            {
                pt.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.PartInventory).WithMany().HasForeignKey(x => x.PartInventoryId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.PartInventoryBatch).WithMany().HasForeignKey(x => x.PartInventoryBatchId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.FromLocation).WithMany().HasForeignKey(x => x.FromLocationId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.ToLocation).WithMany().HasForeignKey(x => x.ToLocationId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.PerformedByUser).WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.IssuedToUser).WithMany().HasForeignKey(x => x.IssuedToUserId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.ReturnedFromUser).WithMany().HasForeignKey(x => x.ReturnedFromUserId).OnDelete(DeleteBehavior.Restrict);
                pt.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartTransactionSerial>(pts =>
            {
                pts.HasOne(x => x.PartTransaction).WithMany(x => x.TransactionSerials).HasForeignKey(x => x.PartTransactionId).OnDelete(DeleteBehavior.Cascade);
                pts.HasOne(x => x.PartSerialNumber).WithMany().HasForeignKey(x => x.PartSerialNumberId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PartReplacement>(pr =>
            {
                pr.HasIndex(x => x.WorkOrderId);
                pr.HasIndex(x => x.AssetId);
                pr.HasIndex(x => x.TenantId);
                pr.HasOne(x => x.WorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.OldAssetComponent).WithMany().HasForeignKey(x => x.OldAssetComponentId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.NewAssetComponent).WithMany().HasForeignKey(x => x.NewAssetComponentId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.NewPart).WithMany().HasForeignKey(x => x.NewPartId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.NewPartSerialNumber).WithMany().HasForeignKey(x => x.NewPartSerialNumberId).OnDelete(DeleteBehavior.Restrict);
                pr.HasOne(x => x.PartTransaction).WithMany().HasForeignKey(x => x.PartTransactionId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaintenanceType>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            });

            modelBuilder.Entity<MaintenanceChecklist>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
                e.HasOne(x => x.MaintenanceType).WithMany().HasForeignKey(x => x.MaintenanceTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaintenanceChecklistItem>(e =>
            {
                e.HasOne(x => x.MaintenanceChecklist).WithMany(x => x.Items).HasForeignKey(x => x.MaintenanceChecklistId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaintenanceChecklistItemOption>(e =>
            {
                e.HasOne(x => x.MaintenanceChecklistItem).WithMany(x => x.Options).HasForeignKey(x => x.MaintenanceChecklistItemId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaintenanceSchedule>(e =>
            {
                e.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.MaintenanceType).WithMany().HasForeignKey(x => x.MaintenanceTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.MaintenanceChecklist).WithMany().HasForeignKey(x => x.MaintenanceChecklistId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PreventiveMaintenanceOccurrence>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.MaintenanceScheduleId, x.ScheduledDate })
                    .IsUnique()
                    .HasFilter("[ScheduledDate] IS NOT NULL");
                e.HasIndex(x => new { x.TenantId, x.MaintenanceScheduleId, x.DueOperatingHours })
                    .IsUnique()
                    .HasFilter("[DueOperatingHours] IS NOT NULL");
                e.HasIndex(x => new { x.TenantId, x.MaintenanceScheduleId, x.DueCycles })
                    .IsUnique()
                    .HasFilter("[DueCycles] IS NOT NULL");
                e.HasOne(x => x.MaintenanceSchedule).WithMany().HasForeignKey(x => x.MaintenanceScheduleId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.MaintenanceType).WithMany().HasForeignKey(x => x.MaintenanceTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.MaintenanceChecklist).WithMany().HasForeignKey(x => x.MaintenanceChecklistId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.StartedByUser).WithMany().HasForeignKey(x => x.StartedByUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PreventiveMaintenanceOccurrenceChecklistItem>(e =>
            {
                e.HasOne(x => x.Occurrence).WithMany(x => x.ChecklistItems).HasForeignKey(x => x.PreventiveMaintenanceOccurrenceId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PreventiveMaintenanceChecklistResponse>(e =>
            {
                e.HasIndex(x => x.OccurrenceChecklistItemId).IsUnique();
                e.HasOne(x => x.Occurrence).WithMany().HasForeignKey(x => x.PreventiveMaintenanceOccurrenceId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.OccurrenceChecklistItem).WithOne(x => x.Response).HasForeignKey<PreventiveMaintenanceChecklistResponse>(x => x.OccurrenceChecklistItemId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IssueCategory>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            });

            modelBuilder.Entity<AssetIssue>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.IssueNumber }).IsUnique();
                e.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.IssueCategory).WithMany().HasForeignKey(x => x.IssueCategoryId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ReportedByUser).WithMany().HasForeignKey(x => x.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ResolvedByUser).WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IssueAttachment>(e =>
            {
                e.HasOne(x => x.AssetIssue).WithMany(x => x.Attachments).HasForeignKey(x => x.AssetIssueId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WorkOrderEntity>(e =>
            {
                e.HasIndex(x => new { x.TenantId, x.WorkOrderNumber }).IsUnique();
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.AssignedToUserId);
                e.HasIndex(x => x.AssetId);
                e.HasIndex(x => x.AssetIssueId);
                e.HasIndex(x => x.PreventiveMaintenanceOccurrenceId);
                e.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AssetIssue).WithMany().HasForeignKey(x => x.AssetIssueId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.PreventiveMaintenanceOccurrence).WithMany().HasForeignKey(x => x.PreventiveMaintenanceOccurrenceId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkOrderDiagnosis>(e =>
            {
                e.HasIndex(x => x.WorkOrderId).IsUnique();
                e.HasOne(x => x.WorkOrder).WithOne(x => x.Diagnosis).HasForeignKey<WorkOrderDiagnosis>(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.DiagnosedByUser).WithMany().HasForeignKey(x => x.DiagnosedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkOrderStatusHistory>(e =>
            {
                e.HasOne(x => x.WorkOrder).WithMany(x => x.StatusHistory).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkOrderAssignmentHistory>(e =>
            {
                e.HasOne(x => x.WorkOrder).WithMany(x => x.AssignmentHistory).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkOrderAttachment>(e =>
            {
                e.HasOne(x => x.WorkOrder).WithMany(x => x.Attachments).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            });

            Seed.Run(modelBuilder);
            // Always add tenant filter but make the filter depend on the DbContext's
            // TenantId property so it is evaluated per DbContext (per request).
            AddTenantFilter(modelBuilder);
            ApplySoftDeleteFilter(modelBuilder);
        }
        public async Task<int> SaveChangesAsync()
        {

            int? userId = null;

            if (httpContextAccessor?.HttpContext != null)
            {
               userId = UserHelper.GetCurrentUserId(httpContextAccessor);
            }

            var addedEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();

            addedEntities.ForEach(e =>
            {
                if (e.Properties.Any(p => p.Metadata.Name == nameof(BaseModel.CreatedOn)))
                {
                    e.Property(nameof(BaseModel.CreatedOn)).CurrentValue = DateTime.UtcNow;
                    e.Property(nameof(BaseModel.ModifiedOn)).CurrentValue = DateTime.UtcNow;
                }

                if (e.Properties.Any(p => p.Metadata.Name == nameof(BaseModel.CreatedById)))
                {
                    e.Property(nameof(BaseModel.CreatedById)).CurrentValue = userId;
                    e.Property(nameof(BaseModel.ModifiedById)).CurrentValue = userId;
                }
                AppendTenancyValue(e);
            });

            var editedEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();

            editedEntities.ForEach(e =>
            {
                if (e.Properties.Any(p => p.Metadata.Name == nameof(BaseModel.ModifiedOn)))
                {
                    e.Property(nameof(BaseModel.ModifiedOn)).CurrentValue = DateTime.UtcNow;
                }

                if (e.Properties.Any(p => p.Metadata.Name == nameof(BaseModel.ModifiedById)))
                {
                    e.Property(nameof(BaseModel.ModifiedById)).CurrentValue = null; //userId;
                }
            });

            return await base.SaveChangesAsync();
        }
        private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            Expression<Func<BaseModel, bool>> filterExpr = bm => !bm.IsDeleted;
            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
            {
                // check if current entity type is child of BaseModel
                if (mutableEntityType.ClrType.IsAssignableTo(typeof(BaseModel)))
                {
                    // modify expression to handle correct child type
                    var parameter = Expression.Parameter(mutableEntityType.ClrType);
                    var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
                    var lambdaExpression = Expression.Lambda(body, parameter);

                    // set filter
                    mutableEntityType.SetQueryFilter(lambdaExpression);
                }
            }
        }

        private void AddTenantFilter(ModelBuilder modelBuilder)
        {
            // Use the DbContext's TenantId property in the filter expression so EF Core
            // will evaluate the filter against the current DbContext instance. This
            // ensures tenant scoping works per request rather than being fixed at
            // model creation time.
            modelBuilder.ApplyGlobalFilters<ITenancyModel>(e =>
                !TenantId.HasValue || (e.TenantId.HasValue && e.TenantId == TenantId));
        }

        private void AppendTenancyValue(EntityEntry entityEntry)
        {
            if (entityEntry.Entity.GetType().GetInterface(typeof(ITenancyModel).Name) != null)
            {
                var tenancy = (ITenancyModel)entityEntry.Entity;
                var providerTenantId = _tenantProvider.GetTenantId();
                if (!tenancy.TenantId.HasValue && providerTenantId.HasValue && providerTenantId != 0)
                    tenancy.TenantId = providerTenantId;
            }
        }
    }
}
