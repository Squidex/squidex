/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxKNumber',
    pure: true,
})
export class KNumberPipe implements PipeTransform {
    public transform(value: number) {
        if (value > 1000) {
            value /= 1000;

            if (value < 10) {
                value = Math.round(value * 10) / 10;
            } else {
                value = Math.round(value);
            }

            return `${value}k`;
        } else if (value < 0) {
            return '';
        } else {
            return value.toString();
        }
    }
}

@Pipe({
    name: 'sqxFileSize',
    pure: true,
})
export class FileSizePipe implements PipeTransform {
    public transform(value: number) {
        return calculateFileSize(value);
    }
}

export function calculateFileSize(value: number, factor = 1024) {
    let u = 0;

    while (value >= factor || -value >= factor) {
        value /= factor;
        u++;
    }

    // eslint-disable-next-line prefer-template
    return (u ? `${value.toFixed(1)} ` : value) + ' kMGTPEZY'[u] + 'B';
}
