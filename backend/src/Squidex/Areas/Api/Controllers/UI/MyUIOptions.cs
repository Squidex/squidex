// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;

namespace Squidex.Areas.Api.Controllers.UI;

public sealed record MyUIOptions
{
    [JsonExtensionData]
    public Dictionary<string, object> More { get; set; } = new Dictionary<string, object>();

    [JsonPropertyName("regexSuggestions")]
    public Dictionary<string, string> RegexSuggestions { get; set; }

    [JsonPropertyName("map")]
    public MapOptions Map { get; set; }

    [JsonPropertyName("referencesDropdownItemCount")]
    public int ReferencesDropdownItemCount { get; set; } = 100;

    [JsonPropertyName("showInfo")]
    public bool ShowInfo { get; set; }

    [JsonPropertyName("hideNews")]
    public bool HideNews { get; set; }

    [JsonPropertyName("hideOnboarding")]
    public bool HideOnboarding { get; set; }

    [JsonPropertyName("hideDateButtons")]
    public bool HideDateButtons { get; set; }

    [JsonPropertyName("hideDateTimeModeButton")]
    public bool HideDateTimeModeButton { get; set; }

    [JsonPropertyName("disableScheduledChanges")]
    public bool DisableScheduledChanges { get; set; }

    [JsonPropertyName("redirectToLogin")]
    public bool RedirectToLogin { get; set; }

    [JsonPropertyName("onlyAdminsCanCreateApps")]
    public bool OnlyAdminsCanCreateApps { get; set; }

    [JsonPropertyName("onlyAdminsCanCreateTeams")]
    public bool OnlyAdminsCanCreateTeams { get; set; }

    public sealed class MapOptions
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("googleMaps")]
        public MapGoogleOptions GoogleMaps { get; set; }
    }

    public sealed class MapGoogleOptions
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }
}
