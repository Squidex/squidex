/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Color } from './color';

const BREWERS = {
      greys: [0xffffff, 0xF0F0F0, 0xbdbdbd, 0x969696, 0x737373, 0x525252, 0x252525, 0x000000],
    oranges: [0xfee6ce, 0xfdd0a2, 0xfdae6b, 0xfd8d3c, 0xf16913, 0xd94801, 0xa63603, 0x7f2704],
       reds: [0xfee0d2, 0xfcbba1, 0xfc9272, 0xfb6a4a, 0xef3b2c, 0xcb181d, 0xa50f15, 0x67000d],
     greens: [0xe5f5e0, 0xc7e9c0, 0xa1d99b, 0x74c476, 0x41ab5d, 0x238b45, 0x006d2c, 0x00441b],
    purples: [0xefedf5, 0xdadaeb, 0xbcbddc, 0x9e9ac8, 0x807dba, 0x6a51a3, 0x54278f, 0x3f007d],
      blues: [0xdeebf7, 0xc6dbef, 0x9ecae1, 0x6baed6, 0x4292c6, 0x2171b5, 0x08519c, 0x08306b]
};

export class ColorPalette {
    constructor(
        public readonly colors: Color[],
        public readonly defaultColor: Color
    ) {
    }

    public static colors(): ColorPalette {
        const colors: Color[] = [];

        for (let key in BREWERS) {
            if (BREWERS.hasOwnProperty(key)) {
                const brewer = BREWERS[key];

                for (let color of brewer) {
                    colors.push(Color.fromNumber(color));
                }
            }
        }

        return new ColorPalette(colors, colors[7]);
    }
}