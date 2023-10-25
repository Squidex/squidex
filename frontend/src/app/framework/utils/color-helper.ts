/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { MathHelper } from './math-helper';
import { StringHelper } from './string-helper';

export module ColorHelper {
    interface RGBColor {
        r: number;
        g: number;
        b: number;
    }

    interface HSVColor {
        h: number;
        s: number;
        v: number;
    }

    interface ColorDefinition {
        regex: RegExp;

        process(bots: RegExpExecArray): RGBColor;
    }

    const ColorDefinitions: ReadonlyArray<ColorDefinition> = [
        {
            regex: /^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/,
            process: (bits) => ({
                r: parseInt(bits[1], 10) / 255,
                g: parseInt(bits[2], 10) / 255,
                b: parseInt(bits[3], 10) / 255,
            }),
        },
        {
            regex: /^(\w{2})(\w{2})(\w{2})$/,
            process: (bits) => ({
                r: parseInt(bits[1], 16) / 255,
                g: parseInt(bits[2], 16) / 255,
                b: parseInt(bits[3], 16) / 255,
            }),
        },
        {
            regex: /^(\w{1})(\w{1})(\w{1})$/,
            process: (bits) => ({
                r: parseInt(bits[1] + bits[1], 16) / 255,
                g: parseInt(bits[2] + bits[2], 16) / 255,
                b: parseInt(bits[3] + bits[3], 16) / 255,
            }),
        },
    ];

    export function parseColor(value: string) {
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

        throw new Error('Color is not in a valid format.');
    }

    export function rgbToHsv({ r, g, b }: RGBColor): HSVColor {
        const max = Math.max(r, g, b);
        const min = Math.min(r, g, b);

        let h = 0;
        const d = max - min;
        const s = max === 0 ? 0 : d / max;
        const v = max;

        if (max === min) {
            h = 0;
        } else {
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }

            h /= 6;
        }

        return { h: h * 360, s, v };
    }

    export function hsvToRgb({ h, s, v }: HSVColor): RGBColor {
        h /= 60;

        const i = Math.floor(h);
        const f = (h - i);
        const p = (v * (1 - s));
        const q = (v * (1 - (f * s)));
        const t = (v * (1 - ((1 - f) * s)));

        function color(r: number, g: number, b: number) {
            return { r, g, b };
        }

        switch (i % 6) {
            case 0:
                return color(v, t, p);
            case 1:
                return color(q, v, p);
            case 2:
                return color(p, v, t);
            case 3:
                return color(p, q, v);
            case 4:
                return color(t, p, v);
            default:
                return color(v, p, q);
        }
    }

    export function colorString({ r, g, b }: RGBColor) {
        let rs = Math.round(r * 255).toString(16);
        let gs = Math.round(g * 255).toString(16);
        let bs = Math.round(b * 255).toString(16);

        if (rs.length === 1) {
            rs = `0${rs}`;
        }
        if (gs.length === 1) {
            gs = `0${gs}`;
        }
        if (bs.length === 1) {
            bs = `0${bs}`;
        }

        return `#${rs}${gs}${bs}`;
    }

    export function fromStringHash(input: string) {
        let color = CACHE_COLORS[input];

        if (!color) {
            const colorHash = StringHelper.hashCode(input) / 10000;
            const colorValue = hsvToRgb({ h: MathHelper.toPositiveDegree(colorHash), s: 0.6, v: 0.6 });

            color = colorString(colorValue);

            CACHE_COLORS[input] = color;
        }

        return color;
    }
}

const CACHE_COLORS: { [email: string]: string } = {};