// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public enum AlexaErrorType
{
    [JsonStringEnumMemberName("INVALID_RESPONSE")]
    InvalidResponse = 0,

    [JsonStringEnumMemberName("DEVICE_COMMUNICATION_ERROR")]
    DeviceCommunicationError,

    [JsonStringEnumMemberName("INTERNAL_ERROR")]
    InternalError,

    [JsonStringEnumMemberName("MEDIA_ERROR_UNKNOWN")]
    MediaErrorUnknown,

    [JsonStringEnumMemberName("MEDIA_ERROR_INVALID_REQUEST")]
    InvalidMediaRequest,

    [JsonStringEnumMemberName("MEDIA_ERROR_SERVICE_UNAVAILABLE")]
    MediaServiceUnavailable,

    [JsonStringEnumMemberName("MEDIA_ERROR_INTERNAL_SERVER_ERROR")]
    InternalServerError,

    [JsonStringEnumMemberName("MEDIA_ERROR_INTERNAL_DEVICE_ERROR")]
    InternalDeviceError,

    [JsonStringEnumMemberName("ALREADY_IN_OPERATION")]
    AlreadyInOperation,

    [JsonStringEnumMemberName("BRIDGE_UNREACHABLE")]
    BridgeUnreachable,

    [JsonStringEnumMemberName("ENDPOINT_BUSY")]
    EndpointBusy,

    [JsonStringEnumMemberName("ENDPOINT_LOW_POWER")]
    EndpointLowPower,

    [JsonStringEnumMemberName("ENDPOINT_UNREACHABLE")]
    EndpointUnreachable,

    [JsonStringEnumMemberName("ENDPOINT_CONTROL_UNAVAILABLE")]
    EndpointControlUnavailable,

    [JsonStringEnumMemberName("EXPIRED_AUTHORIZATION_CREDENTIAL")]
    ExpiredAuthorizationCredential,

    [JsonStringEnumMemberName("FIRMWARE_OUT_OF_DATE")]
    FirmwareOutOfDate,

    [JsonStringEnumMemberName("HARDWARE_MALFUNCTION")]
    HardwareMalfunction,

    [JsonStringEnumMemberName("INSUFFICIENT_PERMISSIONS")]
    InsufficientPermissions,

    [JsonStringEnumMemberName("INTERNAL_SERVICE_ERROR")]
    InternalServiceError,

    [JsonStringEnumMemberName("INVALID_AUTHORIZATION_CREDENTIAL")]
    InvalidAuthorizationCredential,

    [JsonStringEnumMemberName("INVALID_DIRECTIVE")]
    InvalidDirective,

    [JsonStringEnumMemberName("INVALID_VALUE")]
    InvalidValue,

    [JsonStringEnumMemberName("NO_SUCH_ENDPOINT")]
    NoSuchEndpoint,

    [JsonStringEnumMemberName("NOT_CALIBRATED")]
    NotCalibrated,

    [JsonStringEnumMemberName("NOT_IN_OPERATION")]
    NotInOperation,

    [JsonStringEnumMemberName("NOT_SUPPORTED_IN_CURRENT_MODE")]
    NotSupportedInCurrentMode,

    [JsonStringEnumMemberName("NOT_SUPPORTED_WITH_CURRENT_BATTERY_CHARGE_STATE")]
    NotSupportedWithCurrentBatteryChargeState,

    [JsonStringEnumMemberName("PARTNER_APPLICATION_REDIRECTION")]
    PartnerApplicationRedirection,

    [JsonStringEnumMemberName("POWER_LEVEL_NOT_SUPPORTED")]
    PowerLevelNotSupported,

    [JsonStringEnumMemberName("RATE_LIMIT_EXCEEDED")]
    RateLimitExceeded,

    [JsonStringEnumMemberName("TEMPERATURE_VALUE_OUT_OF_RANGE")]
    TemperatureValueOutOfRange,

    [JsonStringEnumMemberName("TOO_MANY_FAILED_ATTEMPTS")]
    TooManyFailedAttempts,

    [JsonStringEnumMemberName("VALUE_OUT_OF_RANGE")]
    ValueOutOfRange,
}
