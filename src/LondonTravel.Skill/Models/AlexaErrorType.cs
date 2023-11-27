// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace MartinCostello.LondonTravel.Skill.Models;

public enum AlexaErrorType
{
    [EnumMember(Value = "INVALID_RESPONSE")]
    InvalidResponse = 0,

    [EnumMember(Value = "DEVICE_COMMUNICATION_ERROR")]
    DeviceCommunicationError,

    [EnumMember(Value = "INTERNAL_ERROR")]
    InternalError,

    [EnumMember(Value = "MEDIA_ERROR_UNKNOWN")]
    MediaErrorUnknown,

    [EnumMember(Value = "MEDIA_ERROR_INVALID_REQUEST")]
    InvalidMediaRequest,

    [EnumMember(Value = "MEDIA_ERROR_SERVICE_UNAVAILABLE")]
    MediaServiceUnavailable,

    [EnumMember(Value = "MEDIA_ERROR_INTERNAL_SERVER_ERROR")]
    InternalServerError,

    [EnumMember(Value = "MEDIA_ERROR_INTERNAL_DEVICE_ERROR")]
    InternalDeviceError,

    [EnumMember(Value = "ALREADY_IN_OPERATION")]
    AlreadyInOperation,

    [EnumMember(Value = "BRIDGE_UNREACHABLE")]
    BridgeUnreachable,

    [EnumMember(Value = "ENDPOINT_BUSY")]
    EndpointBusy,

    [EnumMember(Value = "ENDPOINT_LOW_POWER")]
    EndpointLowPower,

    [EnumMember(Value = "ENDPOINT_UNREACHABLE")]
    EndpointUnreachable,

    [EnumMember(Value = "ENDPOINT_CONTROL_UNAVAILABLE")]
    EndpointControlUnavailable,

    [EnumMember(Value = "EXPIRED_AUTHORIZATION_CREDENTIAL")]
    ExpiredAuthorizationCredential,

    [EnumMember(Value = "FIRMWARE_OUT_OF_DATE")]
    FirmwareOutOfDate,

    [EnumMember(Value = "HARDWARE_MALFUNCTION")]
    HardwareMalfunction,

    [EnumMember(Value = "INSUFFICIENT_PERMISSIONS")]
    InsufficientPermissions,

    [EnumMember(Value = "INTERNAL_SERVICE_ERROR")]
    InternalServiceError,

    [EnumMember(Value = "INVALID_AUTHORIZATION_CREDENTIAL")]
    InvalidAuthorizationCredential,

    [EnumMember(Value = "INVALID_DIRECTIVE")]
    InvalidDirective,

    [EnumMember(Value = "INVALID_VALUE")]
    InvalidValue,

    [EnumMember(Value = "NO_SUCH_ENDPOINT")]
    NoSuchEndpoint,

    [EnumMember(Value = "NOT_CALIBRATED")]
    NotCalibrated,

    [EnumMember(Value = "NOT_IN_OPERATION")]
    NotInOperation,

    [EnumMember(Value = "NOT_SUPPORTED_IN_CURRENT_MODE")]
    NotSupportedInCurrentMode,

    [EnumMember(Value = "NOT_SUPPORTED_WITH_CURRENT_BATTERY_CHARGE_STATE")]
    NotSupportedWithCurrentBatteryChargeState,

    [EnumMember(Value = "PARTNER_APPLICATION_REDIRECTION")]
    PartnerApplicationRedirection,

    [EnumMember(Value = "POWER_LEVEL_NOT_SUPPORTED")]
    PowerLevelNotSupported,

    [EnumMember(Value = "RATE_LIMIT_EXCEEDED")]
    RateLimitExceeded,

    [EnumMember(Value = "TEMPERATURE_VALUE_OUT_OF_RANGE")]
    TemperatureValueOutOfRange,

    [EnumMember(Value = "TOO_MANY_FAILED_ATTEMPTS")]
    TooManyFailedAttempts,

    [EnumMember(Value = "VALUE_OUT_OF_RANGE")]
    ValueOutOfRange,
}
