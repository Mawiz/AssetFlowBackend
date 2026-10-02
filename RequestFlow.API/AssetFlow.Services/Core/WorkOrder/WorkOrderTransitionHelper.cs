using AssetFlow.Common.Enum;

namespace AssetFlow.Services.Core.WorkOrder
{
    internal static class WorkOrderTransitionHelper
    {
        private static readonly Dictionary<int, HashSet<int>> Allowed = new()
        {
            [(int)Enums.WorkOrderStatus.New] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.Submitted,
                (int)Enums.WorkOrderStatus.PendingAssignment,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.Submitted] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.PendingAssignment,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.PendingAssignment] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.Assigned,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.Assigned] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.Accepted,
                (int)Enums.WorkOrderStatus.PendingAssignment,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.Accepted] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.InProgress,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.InProgress] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.WaitingForParts,
                (int)Enums.WorkOrderStatus.WaitingForApproval,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.WaitingForParts] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.InProgress,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.WaitingForApproval] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.Closed,
                (int)Enums.WorkOrderStatus.Rejected
            },
            [(int)Enums.WorkOrderStatus.Rejected] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.Reopened
            },
            [(int)Enums.WorkOrderStatus.Reopened] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.InProgress,
                (int)Enums.WorkOrderStatus.Cancelled
            },
            [(int)Enums.WorkOrderStatus.Completed] = new HashSet<int>
            {
                (int)Enums.WorkOrderStatus.WaitingForApproval
            }
        };

        public static bool CanTransition(int fromStatus, int toStatus) =>
            Allowed.TryGetValue(fromStatus, out var set) && set.Contains(toStatus);

        public static bool IsTerminal(int status) =>
            status == (int)Enums.WorkOrderStatus.Closed ||
            status == (int)Enums.WorkOrderStatus.Cancelled;

        public static bool IsActiveForSource(int status) =>
            !IsTerminal(status);
    }
}
