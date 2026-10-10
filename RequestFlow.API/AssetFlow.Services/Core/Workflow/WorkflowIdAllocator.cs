using AssetFlow.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Services.Core.Workflow
{
    /// <summary>
    /// Application-side sequential IDs for workflow entities that do not use database identity.
    /// AssetFlow has no separate ID service; this mirrors manual max+1 allocation used elsewhere for explicit keys.
    /// </summary>
    internal static class WorkflowIdAllocator
    {
        public static async Task<int> NextModuleIdAsync(ApplicationDbContext context)
        {
            var max = await context.WorkflowModules.AsNoTracking().MaxAsync(m => (int?)m.ModuleId);
            return (max ?? 0) + 1;
        }

        public static async Task<int> NextStageIdAsync(ApplicationDbContext context)
        {
            var max = await context.WorkflowModuleStages.AsNoTracking().MaxAsync(s => (int?)s.StageId);
            return (max ?? 0) + 1;
        }

        public static async Task<int> NextStageActionIdAsync(ApplicationDbContext context)
        {
            var max = await context.WorkflowStageActions.AsNoTracking().MaxAsync(a => (int?)a.StageActionId);
            return (max ?? 0) + 1;
        }
    }
}
