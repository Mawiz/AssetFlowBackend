namespace AssetFlow.Common.Enum
{
    public class Enums
    {
        public enum SecretKeys
        {
            ApplicationRole = 1,
        }

        public enum UserRole
        {
            Supervisor = 1,
            OperatorAllJobs = 2,
            OperatorNonSprayJobs = 3,
            OfficeStaff = 4,
            AccountingStaff = 5,
            Admin = 6,
            Inventory = 7
        }

        public enum OrderBy
        {
            Ascending = 1,
            Descending = 2
        }

        public enum AssetStatus
        {
            Operational = 1,
            UnderMaintenance = 2,
            Breakdown = 3,
            Retired = 4
        }

        public enum AssetCriticality
        {
            Critical = 1,
            High = 2,
            Medium = 3,
            Low = 4
        }

        public enum ExpectedLifeUnit
        {
            Years = 1,
            Hours = 2,
            Cycles = 3,
            Months = 4
        }

        public enum ComponentCurrentStatus
        {
            Active = 1,
            Inactive = 2,
            UnderMaintenance = 3,
            Failed = 4,
            Removed = 5
        }

        public enum PartSerialStatus
        {
            InStock = 1,
            Issued = 2,
            Faulty = 3,
            Quarantine = 4,
            ReturnedToSupplier = 5,
            Scrapped = 6,
            Removed = 7
        }

        /// <summary>Legacy name retained for metadata/API compatibility.</summary>
        public enum PartInventoryStatus
        {
            InStock = 1,
            Issued = 2,
            Reserved = 3,
            Removed = 4,
            Scrapped = 5,
            Faulty = 6,
            Quarantine = 7,
            ReturnedToSupplier = 8
        }

        public enum PartTransactionType
        {
            Receipt = 1,
            Adjustment = 2,
            Transfer = 3,
            Issue = 4,
            Return = 5,
            MarkFaulty = 6,
            Quarantine = 7,
            ReleaseFromQuarantine = 8,
            ReturnToSupplier = 9,
            Scrap = 10
        }
    }
}