// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class representing the configuration for the skill.
/// </summary>
public sealed class SkillConfiguration
{
    /// <summary>
    /// Gets or sets the URL for the skill API.
    /// </summary>
    [Required]
    public string SkillApiUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the skill's ID.
    /// </summary>
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TfL API application Id.
    /// </summary>
    [Required]
    public string TflApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TfL API application key.
    /// </summary>
    [Required]
    public string TflApplicationKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to validate the skill's Id.
    /// </summary>
    public bool VerifySkillId { get; set; }
}
