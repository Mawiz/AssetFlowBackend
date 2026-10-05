using AssetFlow.Common.Helper;

using AssetFlow.Data.Entities.ACL;
using AssetFlow.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Data.Seeds.User
{

    public static class RoleEnumSeed
    {

        public static void Seed(ModelBuilder modelBuilder)
        {

            var dateTime = new DateTime(2021, 11, 26, 0, 0, 0);

            const int userFeatureId = 1;

            const int roleFeatureId = 2;

            const int resourceFeatureId = 3;

            const int tenantFeatureId = 4;

            const int subscriptionFeatureId = 5;

            const int languageFeatureId = 6;

            const int metaDataFeatureId = 7;

            const int locationTypeFeatureId = 46;

            const int locationFeatureId = 47;

            const int assetCategoryFeatureId = 48;

            const int assetTypeFeatureId = 49;

            const int assetFeatureId = 58;

            const int assetComponentFeatureId = 59;

            const int partCategoryFeatureId = 68;

            const int partFeatureId = 69;

            const int partInventoryFeatureId = 70;

            const int partTransactionFeatureId = 71;

            const int supplierFeatureId = 92;
            const int maintenanceTypeFeatureId = 100;
            const int maintenanceChecklistFeatureId = 101;
            const int maintenanceScheduleFeatureId = 102;
            const int preventiveMaintenanceFeatureId = 103;
            const int issueCategoryFeatureId = 120;
            const int assetIssueFeatureId = 121;
            const int workOrderFeatureId = 122;
            const int partReplacementFeatureId = 123;

            modelBuilder.Entity<Resource>()

                .HasData(new List<Resource>

                {

                    new Resource { Id = userFeatureId, ResourceName = "User" },

                    new Resource { Id = roleFeatureId, ResourceName = "Role" },

                    new Resource { Id = resourceFeatureId, ResourceName = "Resource" },

                    new Resource { Id = tenantFeatureId, ResourceName = "Tenant" },

                    new Resource { Id = subscriptionFeatureId, ResourceName = "Subscription" },

                    new Resource { Id = languageFeatureId, ResourceName = "Language" },

                    new Resource { Id = metaDataFeatureId, ResourceName = "MetaData" },

                    new Resource { Id = locationTypeFeatureId, ResourceName = "LocationType" },

                    new Resource { Id = locationFeatureId, ResourceName = "Location" },

                    new Resource { Id = assetCategoryFeatureId, ResourceName = "AssetCategory" },

                    new Resource { Id = assetTypeFeatureId, ResourceName = "AssetType" },

                    new Resource { Id = assetFeatureId, ResourceName = "Asset" },

                    new Resource { Id = assetComponentFeatureId, ResourceName = "AssetComponent" },

                    new Resource { Id = partCategoryFeatureId, ResourceName = "PartCategory" },

                    new Resource { Id = partFeatureId, ResourceName = "Part" },

                    new Resource { Id = partInventoryFeatureId, ResourceName = "PartInventory" },

                    new Resource { Id = partTransactionFeatureId, ResourceName = "PartTransaction" },

                    new Resource { Id = supplierFeatureId, ResourceName = "Supplier" },
                    new Resource { Id = maintenanceTypeFeatureId, ResourceName = "MaintenanceType" },
                    new Resource { Id = maintenanceChecklistFeatureId, ResourceName = "MaintenanceChecklist" },
                    new Resource { Id = maintenanceScheduleFeatureId, ResourceName = "MaintenanceSchedule" },
                    new Resource { Id = preventiveMaintenanceFeatureId, ResourceName = "PreventiveMaintenance" },
                    new Resource { Id = issueCategoryFeatureId, ResourceName = "IssueCategory" },
                    new Resource { Id = assetIssueFeatureId, ResourceName = "AssetIssue" },
                    new Resource { Id = workOrderFeatureId, ResourceName = "WorkOrder" },
                    new Resource { Id = partReplacementFeatureId, ResourceName = "PartReplacement" },

                    new Resource { Id = 8, FeatureId = userFeatureId, ResourceName = Permissions.UserCreate },

                    new Resource { Id = 9, FeatureId = userFeatureId, ResourceName = Permissions.UserUpdate },

                    new Resource { Id = 10, FeatureId = userFeatureId, ResourceName = Permissions.UserView },

                    new Resource { Id = 11, FeatureId = userFeatureId, ResourceName = Permissions.UserList },

                    new Resource { Id = 12, FeatureId = userFeatureId, ResourceName = Permissions.UserToggle },



                    new Resource { Id = 13, FeatureId = roleFeatureId, ResourceName = Permissions.RoleCreate },

                    new Resource { Id = 14, FeatureId = roleFeatureId, ResourceName = Permissions.RoleUpdate },

                    new Resource { Id = 15, FeatureId = roleFeatureId, ResourceName = Permissions.RoleView },

                    new Resource { Id = 16, FeatureId = roleFeatureId, ResourceName = Permissions.RoleList },



                    new Resource { Id = 17, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceCreate },

                    new Resource { Id = 18, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceUpdate },

                    new Resource { Id = 19, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceView },

                    new Resource { Id = 20, FeatureId = resourceFeatureId, ResourceName = Permissions.ResourceList },



                    new Resource { Id = 21, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantCreate },

                    new Resource { Id = 22, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantUpdate },

                    new Resource { Id = 23, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantView },

                    new Resource { Id = 24, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantList },

                    new Resource { Id = 25, FeatureId = tenantFeatureId, ResourceName = Permissions.TenantToggle },



                    new Resource { Id = 26, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionCreate },

                    new Resource { Id = 27, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionUpdate },

                    new Resource { Id = 28, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionView },

                    new Resource { Id = 29, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionList },

                    new Resource { Id = 30, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionToggle },

                    new Resource { Id = 31, FeatureId = subscriptionFeatureId, ResourceName = Permissions.SubscriptionDelete },



                    new Resource { Id = 32, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageCreate },

                    new Resource { Id = 33, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageUpdate },

                    new Resource { Id = 34, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageView },

                    new Resource { Id = 35, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageList },

                    new Resource { Id = 36, FeatureId = languageFeatureId, ResourceName = Permissions.LanguageToggle },



                    new Resource { Id = 37, FeatureId = metaDataFeatureId, ResourceName = Permissions.MetaDataView },

                    new Resource { Id = 38, FeatureId = locationTypeFeatureId, ResourceName = Permissions.LocationTypeView },

                    new Resource { Id = 39, FeatureId = locationTypeFeatureId, ResourceName = Permissions.LocationTypeCreate },

                    new Resource { Id = 40, FeatureId = locationTypeFeatureId, ResourceName = Permissions.LocationTypeUpdate },

                    new Resource { Id = 41, FeatureId = locationTypeFeatureId, ResourceName = Permissions.LocationTypeDelete },

                    new Resource { Id = 42, FeatureId = locationFeatureId, ResourceName = Permissions.LocationView },

                    new Resource { Id = 43, FeatureId = locationFeatureId, ResourceName = Permissions.LocationCreate },

                    new Resource { Id = 44, FeatureId = locationFeatureId, ResourceName = Permissions.LocationUpdate },

                    new Resource { Id = 45, FeatureId = locationFeatureId, ResourceName = Permissions.LocationDelete },

                    new Resource { Id = 50, FeatureId = assetCategoryFeatureId, ResourceName = Permissions.AssetCategoryView },

                    new Resource { Id = 51, FeatureId = assetCategoryFeatureId, ResourceName = Permissions.AssetCategoryCreate },

                    new Resource { Id = 52, FeatureId = assetCategoryFeatureId, ResourceName = Permissions.AssetCategoryUpdate },

                    new Resource { Id = 53, FeatureId = assetCategoryFeatureId, ResourceName = Permissions.AssetCategoryDelete },

                    new Resource { Id = 54, FeatureId = assetTypeFeatureId, ResourceName = Permissions.AssetTypeView },

                    new Resource { Id = 55, FeatureId = assetTypeFeatureId, ResourceName = Permissions.AssetTypeCreate },

                    new Resource { Id = 56, FeatureId = assetTypeFeatureId, ResourceName = Permissions.AssetTypeUpdate },

                    new Resource { Id = 57, FeatureId = assetTypeFeatureId, ResourceName = Permissions.AssetTypeDelete },

                    new Resource { Id = 60, FeatureId = assetFeatureId, ResourceName = Permissions.AssetView },

                    new Resource { Id = 61, FeatureId = assetFeatureId, ResourceName = Permissions.AssetCreate },

                    new Resource { Id = 62, FeatureId = assetFeatureId, ResourceName = Permissions.AssetUpdate },

                    new Resource { Id = 63, FeatureId = assetFeatureId, ResourceName = Permissions.AssetDelete },

                    new Resource { Id = 64, FeatureId = assetComponentFeatureId, ResourceName = Permissions.AssetComponentView },

                    new Resource { Id = 65, FeatureId = assetComponentFeatureId, ResourceName = Permissions.AssetComponentCreate },

                    new Resource { Id = 66, FeatureId = assetComponentFeatureId, ResourceName = Permissions.AssetComponentUpdate },

                    new Resource { Id = 67, FeatureId = assetComponentFeatureId, ResourceName = Permissions.AssetComponentDelete },

                    new Resource { Id = 72, FeatureId = partCategoryFeatureId, ResourceName = Permissions.PartCategoryView },

                    new Resource { Id = 73, FeatureId = partCategoryFeatureId, ResourceName = Permissions.PartCategoryCreate },

                    new Resource { Id = 74, FeatureId = partCategoryFeatureId, ResourceName = Permissions.PartCategoryUpdate },

                    new Resource { Id = 75, FeatureId = partCategoryFeatureId, ResourceName = Permissions.PartCategoryDelete },

                    new Resource { Id = 76, FeatureId = partFeatureId, ResourceName = Permissions.PartView },

                    new Resource { Id = 77, FeatureId = partFeatureId, ResourceName = Permissions.PartCreate },

                    new Resource { Id = 78, FeatureId = partFeatureId, ResourceName = Permissions.PartUpdate },

                    new Resource { Id = 79, FeatureId = partFeatureId, ResourceName = Permissions.PartDelete },

                    new Resource { Id = 80, FeatureId = partInventoryFeatureId, ResourceName = Permissions.PartInventoryView },

                    new Resource { Id = 81, FeatureId = partInventoryFeatureId, ResourceName = Permissions.PartInventoryCreate },

                    new Resource { Id = 82, FeatureId = partInventoryFeatureId, ResourceName = Permissions.PartInventoryUpdate },

                    new Resource { Id = 83, FeatureId = partInventoryFeatureId, ResourceName = Permissions.PartInventoryDelete },

                    new Resource { Id = 84, FeatureId = partTransactionFeatureId, ResourceName = Permissions.PartTransactionView },

                    new Resource { Id = 85, FeatureId = partTransactionFeatureId, ResourceName = Permissions.PartTransactionCreate },

                    new Resource { Id = 86, FeatureId = partTransactionFeatureId, ResourceName = Permissions.PartTransactionUpdate },

                    new Resource { Id = 87, FeatureId = partTransactionFeatureId, ResourceName = Permissions.PartTransactionDelete },

                    new Resource { Id = 93, FeatureId = supplierFeatureId, ResourceName = Permissions.SupplierView },

                    new Resource { Id = 94, FeatureId = supplierFeatureId, ResourceName = Permissions.SupplierCreate },

                    new Resource { Id = 95, FeatureId = supplierFeatureId, ResourceName = Permissions.SupplierUpdate },

                    new Resource { Id = 96, FeatureId = supplierFeatureId, ResourceName = Permissions.SupplierDelete },

                    new Resource { Id = 104, FeatureId = maintenanceTypeFeatureId, ResourceName = Permissions.MaintenanceTypeView },
                    new Resource { Id = 105, FeatureId = maintenanceTypeFeatureId, ResourceName = Permissions.MaintenanceTypeCreate },
                    new Resource { Id = 106, FeatureId = maintenanceTypeFeatureId, ResourceName = Permissions.MaintenanceTypeUpdate },
                    new Resource { Id = 107, FeatureId = maintenanceTypeFeatureId, ResourceName = Permissions.MaintenanceTypeDelete },

                    new Resource { Id = 108, FeatureId = maintenanceChecklistFeatureId, ResourceName = Permissions.MaintenanceChecklistView },
                    new Resource { Id = 109, FeatureId = maintenanceChecklistFeatureId, ResourceName = Permissions.MaintenanceChecklistCreate },
                    new Resource { Id = 110, FeatureId = maintenanceChecklistFeatureId, ResourceName = Permissions.MaintenanceChecklistUpdate },
                    new Resource { Id = 111, FeatureId = maintenanceChecklistFeatureId, ResourceName = Permissions.MaintenanceChecklistDelete },

                    new Resource { Id = 112, FeatureId = maintenanceScheduleFeatureId, ResourceName = Permissions.MaintenanceScheduleView },
                    new Resource { Id = 113, FeatureId = maintenanceScheduleFeatureId, ResourceName = Permissions.MaintenanceScheduleCreate },
                    new Resource { Id = 114, FeatureId = maintenanceScheduleFeatureId, ResourceName = Permissions.MaintenanceScheduleUpdate },
                    new Resource { Id = 115, FeatureId = maintenanceScheduleFeatureId, ResourceName = Permissions.MaintenanceScheduleDelete },

                    new Resource { Id = 116, FeatureId = preventiveMaintenanceFeatureId, ResourceName = Permissions.PreventiveMaintenanceView },
                    new Resource { Id = 117, FeatureId = preventiveMaintenanceFeatureId, ResourceName = Permissions.PreventiveMaintenanceUpdate },
                    new Resource { Id = 118, FeatureId = preventiveMaintenanceFeatureId, ResourceName = Permissions.PreventiveMaintenanceComplete },
                    new Resource { Id = 119, FeatureId = preventiveMaintenanceFeatureId, ResourceName = Permissions.PreventiveMaintenanceGenerate },

                    new Resource { Id = 128, FeatureId = issueCategoryFeatureId, ResourceName = Permissions.IssueCategoryView },
                    new Resource { Id = 129, FeatureId = issueCategoryFeatureId, ResourceName = Permissions.IssueCategoryCreate },
                    new Resource { Id = 130, FeatureId = issueCategoryFeatureId, ResourceName = Permissions.IssueCategoryUpdate },
                    new Resource { Id = 131, FeatureId = issueCategoryFeatureId, ResourceName = Permissions.IssueCategoryDelete },

                    new Resource { Id = 132, FeatureId = assetIssueFeatureId, ResourceName = Permissions.AssetIssueView },
                    new Resource { Id = 133, FeatureId = assetIssueFeatureId, ResourceName = Permissions.AssetIssueCreate },
                    new Resource { Id = 134, FeatureId = assetIssueFeatureId, ResourceName = Permissions.AssetIssueUpdate },
                    new Resource { Id = 135, FeatureId = assetIssueFeatureId, ResourceName = Permissions.AssetIssueDelete },

                    new Resource { Id = 136, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderView },
                    new Resource { Id = 137, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderCreate },
                    new Resource { Id = 138, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderUpdate },
                    new Resource { Id = 139, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderDelete },
                    new Resource { Id = 140, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderAssign },
                    new Resource { Id = 141, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderReassign },
                    new Resource { Id = 142, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderAccept },
                    new Resource { Id = 143, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderStart },
                    new Resource { Id = 144, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderPause },
                    new Resource { Id = 145, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderResume },
                    new Resource { Id = 146, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderComplete },
                    new Resource { Id = 147, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderApprove },
                    new Resource { Id = 148, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderReject },
                    new Resource { Id = 149, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderReopen },
                    new Resource { Id = 150, FeatureId = workOrderFeatureId, ResourceName = Permissions.WorkOrderCancel },

                    new Resource { Id = 151, FeatureId = partReplacementFeatureId, ResourceName = Permissions.PartReplacementView },
                    new Resource { Id = 152, FeatureId = partReplacementFeatureId, ResourceName = Permissions.PartReplacementCreate },
                    new Resource { Id = 153, FeatureId = partReplacementFeatureId, ResourceName = Permissions.PartReplacementValidate },
                    new Resource { Id = 154, FeatureId = partReplacementFeatureId, ResourceName = Permissions.PartReplacementReplace },
                    new Resource { Id = 155, FeatureId = partReplacementFeatureId, ResourceName = Permissions.PartReplacementDelete }

                });

            //modelBuilder.Entity<ApplicationRole>()
            //    .HasData(new List<ApplicationRole>
            //    {
            //        new ApplicationRole
            //        {
            //            Id = 1,
            //            Order = 1,
            //            Name = "CEO Manufactureur",
            //            NormalizedName = "CEO",
            //            DisplayName = "CEO",
            //            Description = "CEO ",
            //            ConcurrencyStamp = string.Empty,
            //            CreatedOn = dateTime,
            //            ModifiedOn = dateTime
            //        }
            //    });

            //var adminRoleResources = Enumerable.Range(8, 30)

            //    .Select((resourceId, index) => new RoleResource

            //    {

            //        Id = index + 1,

            //        ApplicationRoleId = 1,

            //        ResourceId = resourceId

            //    })

            //    .ToList();



            //modelBuilder.Entity<RoleResource>().HasData(adminRoleResources);

        }

    }

}


