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
import { ColorHelper } from '@app/framework/utils/color-helper';

@Pipe({
    name: 'sqxDarken',
    pure: true,
})
export class DarkenPipe implements PipeTransform {
    public transform(value: string, percentage: number): any {
        const rgb = ColorHelper.parseColor(value);
        const hsv = ColorHelper.rgbToHsv(rgb);

        hsv.v = Math.max(0, hsv.v * (1 - (percentage / 100)));

        return ColorHelper.colorString(ColorHelper.hsvToRgb(hsv));
    }
}

@Pipe({
    name: 'sqxLighten',
    pure: true,
})
export class LightenPipe implements PipeTransform {
    public transform(value: string, percentage: number): any {
        const rgb = ColorHelper.parseColor(value);
        const hsv = ColorHelper.rgbToHsv(rgb);

        hsv.v = Math.min(1, hsv.v * (1 + (percentage / 100)));

        return ColorHelper.colorString(ColorHelper.hsvToRgb(hsv));
    }
}

@Pipe({
    name: 'sqxStringColor',
    pure: true,
})
export class StringColorPipe implements PipeTransform {
    public transform(value: string) {
        return ColorHelper.fromStringHash(value);
    }
}
