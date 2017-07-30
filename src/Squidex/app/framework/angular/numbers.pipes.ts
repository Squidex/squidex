/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxKNumber',
    pure: true
})
export class KNumberPipe implements PipeTransform {
    public transform(value: number) {
        if (value > 1000) {
            value = value / 1000;

            if (value < 10) {
                value = Math.round(value * 10) / 10;
            } else {
                value = Math.round(value);
            }

            return value + 'k';
        } else if (value < 0) {
            return '';
        } else {
            return value.toString();
        }
    }
}

@Pipe({
    name: 'sqxFileSize',
    pure: true
})
export class FileSizePipe implements PipeTransform {
    public transform(value: number) {
        let u = 0, s = 1024;

        while (value >= s || -value >= s) {
            value /= s;
            u++;
        }

        return (u ? value.toFixed(1) + ' ' : value) + ' kMGTPEZY'[u] + 'B';
    }
}

