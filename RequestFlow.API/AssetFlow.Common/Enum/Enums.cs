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
    }
}