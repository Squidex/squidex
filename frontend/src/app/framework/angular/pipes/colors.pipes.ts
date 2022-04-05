/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable one-var-declaration-per-line */
/* eslint-disable one-var */
/* eslint-disable no-sequences */

import { Pipe, PipeTransform } from '@angular/core';

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

function parseColor(value: string) {
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

function rgbToHsv({ r, g, b }: RGBColor): HSVColor {
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

    return { h, s, v };
}

function hsvToRgb({ h, s, v }: HSVColor): RGBColor {
    let r = 0, g = 0, b = 0;

    const i = Math.floor(h * 6);
    const f = h * 6 - i;
    const p = v * (1 - s);
    const q = v * (1 - f * s);
    const t = v * (1 - (1 - f) * s);

    switch (i % 6) {
        case 0: r = v, g = t, b = p; break;
        case 1: r = q, g = v, b = p; break;
        case 2: r = p, g = v, b = t; break;
        case 3: r = p, g = q, b = v; break;
        case 4: r = t, g = p, b = v; break;
        case 5: r = v, g = p, b = q; break;
    }

    return { r, g, b };
}

function colorString({ r, g, b }: RGBColor) {
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

@Pipe({
    name: 'sqxDarken',
    pure: true,
})
export class DarkenPipe implements PipeTransform {
    public transform(value: string, percentage: number): any {
        const rgb = parseColor(value);
        const hsv = rgbToHsv(rgb);

        hsv.v = Math.max(0, hsv.v * (1 - (percentage / 100)));

        return colorString(hsvToRgb(hsv));
    }
}

@Pipe({
    name: 'sqxLighten',
    pure: true,
})
export class LightenPipe implements PipeTransform {
    public transform(value: string, percentage: number): any {
        const rgb = parseColor(value);
        const hsv = rgbToHsv(rgb);

        hsv.v = Math.min(1, hsv.v * (1 + (percentage / 100)));

        return colorString(hsvToRgb(hsv));
    }
}
