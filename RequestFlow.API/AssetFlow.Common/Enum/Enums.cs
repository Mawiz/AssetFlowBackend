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
    }
}