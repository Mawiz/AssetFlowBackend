using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using AssetFlow.Common.Helper;
using AssetFlow.Data.Entities;
using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Entities.Configurations;
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
                !TenantId.HasValue || e.TenantId == TenantId);
        }

        private void AppendTenancyValue(EntityEntry entityEntry)
        {
            if (entityEntry.Entity.GetType().GetInterface(typeof(ITenancyModel).Name) != null)
            {
                if (((ITenancyModel)entityEntry.Entity).TenantId == 0 && _tenantProvider.GetTenantId() != 0)
                    ((ITenancyModel)entityEntry.Entity).TenantId = _tenantProvider.GetTenantId();
            }
        }
    }
}
