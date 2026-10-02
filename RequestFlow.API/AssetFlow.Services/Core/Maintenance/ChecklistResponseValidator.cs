using AssetFlow.Common.Enum;
using AssetFlow.Data.Entities.Maintenance;
using System.Globalization;

namespace AssetFlow.Services.Core.Maintenance
{
    public static class ChecklistResponseValidator
    {
        public static string? ValidateItem(PreventiveMaintenanceOccurrenceChecklistItem item, string? responseValue, decimal? numericValue)
        {
            if (!item.IsRequired && string.IsNullOrWhiteSpace(responseValue) && !numericValue.HasValue) return null;

            var type = item.ResponseType;
            if (type == (int)Enums.ChecklistResponseType.Numeric)
            {
                if (!numericValue.HasValue) return $"Numeric response required for '{item.ItemText}'.";
                return null;
            }

            if (string.IsNullOrWhiteSpace(responseValue))
                return item.IsRequired ? $"Response required for '{item.ItemText}'." : null;

            var value = responseValue.Trim();
            switch (type)
            {
                case (int)Enums.ChecklistResponseType.PassFail:
                    if (!value.Equals("Pass", StringComparison.OrdinalIgnoreCase) && !value.Equals("Fail", StringComparison.OrdinalIgnoreCase))
                        return $"'{item.ItemText}' must be Pass or Fail.";
                    break;
                case (int)Enums.ChecklistResponseType.YesNo:
                    if (!value.Equals("Yes", StringComparison.OrdinalIgnoreCase) && !value.Equals("No", StringComparison.OrdinalIgnoreCase))
                        return $"'{item.ItemText}' must be Yes or No.";
                    break;
                case (int)Enums.ChecklistResponseType.Selection:
                    var options = ParseOptions(item.OptionsJson);
                    if (options.Count > 0 && !options.Any(o => o.Equals(value, StringComparison.OrdinalIgnoreCase)))
                        return $"'{item.ItemText}' must be one of the configured options.";
                    break;
            }
            return null;
        }

        public static List<string> ParseOptions(string? optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson)) return new List<string>();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
