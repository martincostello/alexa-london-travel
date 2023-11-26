// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Skill;

/// <summary>
/// A class that validates instances of the <see cref="SkillConfiguration"/> class. This class cannot be inherited.
/// </summary>
[OptionsValidator]
internal sealed partial class ValidateSkillConfiguration : IValidateOptions<SkillConfiguration>
{
}
