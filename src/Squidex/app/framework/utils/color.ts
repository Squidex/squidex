/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

interface IColorDefinition {
    regex: RegExp;

    process(bots: RegExpExecArray): Color;
}

const ColorDefinitions: IColorDefinition[] = [
    {
        regex: /^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/,
        process: function (bits) {
            return new Color(
                parseInt(bits[1], 10) / 255,
                parseInt(bits[2], 10) / 255,
                parseInt(bits[3], 10) / 255);
        }
    },
    {
        regex: /^(\w{2})(\w{2})(\w{2})$/,
        process: function (bits) {
            return new Color(
                parseInt(bits[1], 16) / 255,
                parseInt(bits[2], 16) / 255,
                parseInt(bits[3], 16) / 255);
        }
    },
    {
        regex: /^(\w{1})(\w{1})(\w{1})$/,
        process: function (bits) {
            return new Color(
                parseInt(bits[1] + bits[1], 16) / 255,
                parseInt(bits[2] + bits[2], 16) / 255,
                parseInt(bits[3] + bits[3], 16) / 255);
        }
    }
];

export class Color {
    public static readonly BLACK = new Color(0, 0, 0);
    public static readonly WHITE = new Color(1, 1, 1);
    public static readonly GREEN = new Color(0, 1, 0);
    public static readonly BLUE = new Color(0, 0, 1);
    public static readonly RED = new Color(1, 0, 0);

    public readonly r: number;
    public readonly g: number;
    public readonly b: number;

    public get luminance() {
        return (this.r + this.r + this.b + this.g + this.g + this.g) / 6;
    }

    constructor(r: number, g: number, b: number) {
        this.r = Math.min(1, Math.max(0, r));
        this.g = Math.min(1, Math.max(0, g));
        this.b = Math.min(1, Math.max(0, b));

        Object.freeze(this);
    }

    public eq(v: Color): boolean {
        return this.r === v.r && this.g === v.g && this.b === v.b;
    }

    public ne(v: Color): boolean {
        return this.r !== v.r || this.g !== v.g || this.b !== v.b;
    }

    public toNumber(): number {
        return ((this.r * 255) << 16) + ((this.g * 255) << 8) + (this.b * 255);
    }

    public toString(): string {
        let r = Math.round(this.r * 255).toString(16);
        let g = Math.round(this.g * 255).toString(16);
        let b = Math.round(this.b * 255).toString(16);

        if (r.length === 1) {
            r = '0' + r;
        }
        if (g.length === 1) {
            g = '0' + g;
        }
        if (b.length === 1) {
            b = '0' + b;
        }

        return '#' + r + g + b;
    }

    public static fromHex(r: number, g: number, b: number): Color {
        return new Color(
            parseInt('' + r, 16) / 255,
            parseInt('' + g, 16) / 255,
            parseInt('' + b, 16) / 255);
    }

    public static fromNumber(rgb: number): Color {
        return new Color(
            ((rgb >> 16) & 0xff) / 255,
            ((rgb >> 8) & 0xff) / 255,
            ((rgb >> 0) & 0xff) / 255);
    }

    public static fromValue(value: number | string | Color): Color {
        if (typeof value === 'string') {
            return Color.fromString(value);
        } else if (typeof value === 'number') {
            return Color.fromNumber(value);
        } else {
            return value;
        }
    }

    public static fromString(value: string) {
        if (value.charAt(0) === '#') {
            value = value.substr(1, 6);
        }

        value = value.replace(/ /g, '').toLowerCase();

        for (let colorDefinition of ColorDefinitions) {
            const bits = colorDefinition.regex.exec(value);

            if (bits) {
                return colorDefinition.process(bits);
            }
        }

        throw new Error('Color is not in a valid format.');
    }

    public static fromHsv(h: number, s: number, v: number): Color {
        const hi = Math.floor(h / 60) % 6;

        const f = (h / 60) - Math.floor(h / 60);

        v = v * 255;

        const p = (v * (1 - s));
        const q = (v * (1 - (f * s)));
        const t = (v * (1 - ((1 - f) * s)));

        switch (hi) {
            case 0:
                return new Color(v, t, p);
            case 1:
                return new Color(q, v, p);
            case 2:
                return new Color(p, v, t);
            case 3:
                return new Color(p, q, v);
            case 4:
                return new Color(t, p, v);
            default:
                return new Color(v, p, q);
        }
    }

    public static fromHsl(h: number, s: number, l: number): Color {
        let r = 0, g = 0, b = 0;

        h = h / 360;

        if (s === 0) {
            r = g = b = l;
        } else {
            const hue2rgb = (p: number, q: number, t: number) => {
                if (t < 0) {
                    t += 1;
                }

                if (t > 1) {
                    t -= 1;
                }
                if (t < 1 / 6) {
                    return p + (q - p) * 6 * t;
                }
                if (t < 1 / 2) {
                    return q;
                }
                if (t < 2 / 3) {
                    return p + (q - p) * (2 / 3 - t) * 6;
                }
                return p;
            };

            const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            const p = 2 * l - q;

            r = hue2rgb(p, q, h + 1 / 3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1 / 3);
        }

        return new Color(r, g, b);
    }
}