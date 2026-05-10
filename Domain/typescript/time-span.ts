/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { IUtf8SpanFormattable } from "./i-utf8-span-formattable";

export interface TimeSpan {
    readonly zero: TimeSpan;
    readonly maxValue: TimeSpan;
    readonly minValue: TimeSpan;
    readonly nanosecondsPerTick: number;
    readonly ticksPerMicrosecond: number;
    readonly ticksPerMillisecond: number;
    readonly ticksPerSecond: number;
    readonly ticksPerMinute: number;
    readonly ticksPerHour: number;
    readonly ticksPerDay: number;
    readonly microsecondsPerMillisecond: number;
    readonly microsecondsPerSecond: number;
    readonly microsecondsPerMinute: number;
    readonly microsecondsPerHour: number;
    readonly microsecondsPerDay: number;
    readonly millisecondsPerSecond: number;
    readonly millisecondsPerMinute: number;
    readonly millisecondsPerHour: number;
    readonly millisecondsPerDay: number;
    readonly secondsPerMinute: number;
    readonly secondsPerHour: number;
    readonly secondsPerDay: number;
    readonly minutesPerHour: number;
    readonly minutesPerDay: number;
    readonly hoursPerDay: number;
    ticks: number;
    days: number;
    hours: number;
    milliseconds: number;
    microseconds: number;
    nanoseconds: number;
    minutes: number;
    seconds: number;
    totalDays: number;
    totalHours: number;
    totalMilliseconds: number;
    totalMicroseconds: number;
    totalNanoseconds: number;
    totalMinutes: number;
    totalSeconds: number;
}
