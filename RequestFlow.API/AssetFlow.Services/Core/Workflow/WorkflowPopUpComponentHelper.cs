using static AssetFlow.Common.Enum.Enums;

namespace AssetFlow.Services.Core.Workflow
{
    public static class WorkflowPopUpComponentHelper
    {
        public static (bool Ok, string? Error, string? Normalized) NormalizeStorage(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return (true, null, null);

            var parse = ParseIds(raw);
            if (!parse.Ok)
                return (false, parse.Error, null);

            if (parse.Ids.Count == 0)
                return (true, null, null);

            var normalized = string.Join(",", parse.Ids);
            return (true, null, normalized);
        }

        public static (bool Ok, string? Error, IReadOnlyList<ActionPopUpRenderingComponents> Ids) ParseIds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return (true, null, Array.Empty<ActionPopUpRenderingComponents>());

            var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<ActionPopUpRenderingComponents>();
            foreach (var part in parts)
            {
                if (!TryParseComponentToken(part, out var component))
                    return (false, $"Invalid popup component '{part}'.", Array.Empty<ActionPopUpRenderingComponents>());
                if (!list.Contains(component))
                    list.Add(component);
            }

            list.Sort((a, b) => ((int)a).CompareTo((int)b));
            return (true, null, list);
        }

        public static bool RequiresComment(IReadOnlyList<ActionPopUpRenderingComponents> ids) =>
            ids.Contains(ActionPopUpRenderingComponents.Comment);

        public static bool RequiresSolution(IReadOnlyList<ActionPopUpRenderingComponents> ids) =>
            ids.Contains(ActionPopUpRenderingComponents.SolutionText);

        public static bool RequiresSpecificPerson(IReadOnlyList<ActionPopUpRenderingComponents> ids) =>
            ids.Contains(ActionPopUpRenderingComponents.SpecificPerson);

        private static bool TryParseComponentToken(string token, out ActionPopUpRenderingComponents component)
        {
            component = default;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (int.TryParse(token, out var id) && Enum.IsDefined(typeof(ActionPopUpRenderingComponents), id))
            {
                component = (ActionPopUpRenderingComponents)id;
                return true;
            }

            var key = token.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
            if (Enum.TryParse<ActionPopUpRenderingComponents>(key, ignoreCase: true, out component))
                return true;

            return key.ToLowerInvariant() switch
            {
                "comment" => Assign(ActionPopUpRenderingComponents.Comment, out component),
                "solution" or "solutiontext" => Assign(ActionPopUpRenderingComponents.SolutionText, out component),
                "specificperson" or "specificpersonuser" => Assign(ActionPopUpRenderingComponents.SpecificPerson, out component),
                _ => false
            };
        }

        private static bool Assign(ActionPopUpRenderingComponents value, out ActionPopUpRenderingComponents component)
        {
            component = value;
            return true;
        }
    }
}
