/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */
/* eslint-disable no-bitwise */

import { Types } from './types';

interface ColorDefinition {
    regex: RegExp;

    process(bots: RegExpExecArray): Color;
}

export interface Color {
    r: number;
    g: number;
    b: number;
    a: number;
}

const ColorDefinitions: ReadonlyArray<ColorDefinition> = [
    {
        regex: /^rgba\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3}),\s*([\d\.]{1,})\)$/,
        process: (bits) => {
            return createColor(
                parseInt(bits[1], 10) / 255,
                parseInt(bits[2], 10) / 255,
                parseInt(bits[3], 10) / 255,
                parseFloat(bits[4]));
        },
    }, {
        regex: /^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/,
        process: (bits) => {
            return createColor(
                parseInt(bits[1], 10) / 255,
                parseInt(bits[2], 10) / 255,
                parseInt(bits[3], 10) / 255);
        },
    }, {
        regex: /^(\w{2})(\w{2})(\w{2})$/,
        process: (bits) => {
            return createColor(
                parseInt(bits[1], 16) / 255,
                parseInt(bits[2], 16) / 255,
                parseInt(bits[3], 16) / 255);
        },
    }, {
        regex: /^(\w{1})(\w{1})(\w{1})$/,
        process: (bits) => {
            return createColor(
                parseInt(bits[1] + bits[1], 16) / 255,
                parseInt(bits[2] + bits[2], 16) / 255,
                parseInt(bits[3] + bits[3], 16) / 255);
        },
    },
];

export module MathHelper {
    export const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

    const CRC32_TABLE: ReadonlyArray<number> = createCrc32Table();

    // eslint-disable-next-line no-inner-declarations
    function createCrc32Table() {
        const crc: number[] = [];

        for (let n = 0; n < 256; n++) {
            let c = n;

            for (let k = 0; k < 8; k++) {
                c = ((c & 1) ? (0xEDB88320 ^ (c >>> 1)) : (c >>> 1));
            }

            crc[n] = c;
        }

        return crc;
    }

    export function crc32(str: string): number {
        let crc = 0 ^ (-1);

        for (let i = 0; i < str.length; i++) {
            crc = (crc >>> 8) ^ CRC32_TABLE[(crc ^ str.charCodeAt(i)) & 0xFF];
        }

        return (crc ^ (-1)) >>> 0;
    }

    export function guid(): string {
        return `${s4() + s4()}-${s4()}-${s4()}-${s4()}-${s4()}${s4()}${s4()}`;
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

    export function parseColor(value: string): Color | undefined {
        if (!Types.isString(value)) {
            return undefined;
        }

        if (value.charAt(0) === '#') {
            value = value.substring(1, 7);
        }

        value = value.replace(/ /g, '').toLowerCase();

        for (const colorDefinition of ColorDefinitions) {
            const bits = colorDefinition.regex.exec(value);

            if (bits) {
                return colorDefinition.process(bits);
            }
        }

        return undefined;
    }

    export function randomColor() {
        return colorToString(colorFromHsv(Math.random() * 360, 0.9, 0.9));
    }

    export function colorToString(color: Color): string {
        let r = Math.round(color.r * 255).toString(16);
        let g = Math.round(color.g * 255).toString(16);
        let b = Math.round(color.b * 255).toString(16);

        if (r.length === 1) {
            r = `0${r}`;
        }
        if (g.length === 1) {
            g = `0${g}`;
        }
        if (b.length === 1) {
            b = `0${b}`;
        }

        return `#${r}${g}${b}`;
    }

    export function colorFromHsv(h: number, s: number, v: number): Color {
        const hi = Math.floor(h / 60) % 6;

        const f = (h / 60) - Math.floor(h / 60);

        const p = (v * (1 - s));
        const q = (v * (1 - (f * s)));
        const t = (v * (1 - ((1 - f) * s)));

        switch (hi) {
            case 0:
                return createColor(v, t, p);
            case 1:
                return createColor(q, v, p);
            case 2:
                return createColor(p, v, t);
            case 3:
                return createColor(p, q, v);
            case 4:
                return createColor(t, p, v);
            default:
                return createColor(v, p, q);
        }
    }

    export function toLuminance(color: Color) {
        if (!color) {
            return 1;
        }

        return (0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b) / color.a;
    }
}

export function createColor(r: number, g: number, b: number, a = 1) {
    return { r, g, b, a };
}
