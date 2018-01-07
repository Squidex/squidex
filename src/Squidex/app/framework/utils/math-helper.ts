/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* tslint:disable: no-bitwise */

export module MathHelper {
    export const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

    const CRC32_TABLE: number[] = [];

    function ensureCrc32Table() {
        for (let n = 0; n < 256; n++) {
            let c = n;

            for (let k = 0; k < 8; k++) {
                c = ((c & 1) ? (0xEDB88320 ^ (c >>> 1)) : (c >>> 1));
            }

            CRC32_TABLE[n] = c;
        }
    }

    ensureCrc32Table();

    export function crc32(str: string): number {
        let crc = 0 ^ (-1);

        for (let i = 0; i < str.length; i++) {
            crc = (crc >>> 8) ^ CRC32_TABLE[(crc ^ str.charCodeAt(i)) & 0xFF];
        }

        return (crc ^ (-1)) >>> 0;
    }

    export function guid(): string {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }

    export function s4(): string {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }

    export function toRad(degree: number): number {
        return degree * Math.PI / 180;
    }

    export function toDegree(rad: number): number {
        return rad * 180 / Math.PI;
    }

    export function simpleCos(degree: number): number {
        return MathHelper.isSinusRange(MathHelper.toPositiveDegree(degree)) ? 0 : 1;
    }

    export function simpleSin(degree: number) {
        return MathHelper.isSinusRange(MathHelper.toPositiveDegree(degree)) ? 1 : 0;
    }

    export function isSinusRange(degree: number): boolean {
        return (degree >= 45 && degree <= 135) || (degree >= 225 && degree <= 315);
    }

    export function roundToMultipleOf(value: number, factor: number): number {
        return Math.round(value / factor) * factor;
    }

    export function roundToMultipleOfTwo(value: number): number {
        return Math.round(value / 2) * 2;
    }

    export function toPositiveDegree(degree: number): number {
        while (degree < 0) {
            degree += 360;
        }

        while (degree >= 360) {
            degree -= 360;
        }

        return degree;
    }
}