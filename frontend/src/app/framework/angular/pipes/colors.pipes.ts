/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { ColorHelper } from '@app/framework/utils/color-helper';

@Pipe({
    name: 'sqxDarken',
    pure: true,
    standalone: true,
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
    standalone: true,
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
    standalone: true,
})
export class StringColorPipe implements PipeTransform {
    public transform(value?: string) {
        if (!value) {
            return 'transparent';
        }

        return ColorHelper.fromStringHash(value);
    }
}
