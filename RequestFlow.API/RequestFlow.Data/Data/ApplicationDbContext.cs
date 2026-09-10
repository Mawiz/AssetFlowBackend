using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using RequestFlow.Common.Helper;
using RequestFlow.Data.Entities;
using RequestFlow.Data.Entities.ACL;
using RequestFlow.Data.Entities.Configurations;
using RequestFlow.Data.Entities.Tenant;
using RequestFlow.Data.Extensions;
using RequestFlow.Data.Identity;
using RequestFlow.Data.Provider;
using System.Linq.Expressions;


namespace RequestFlow.Data.Data
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
        #endregion Tenant

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

            //Seed.Run(modelBuilder);
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
