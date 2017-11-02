/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxKeys',
    pure: true
})
export class KeysPipe implements PipeTransform {
    public transform(value: any, args: any[] = null): any {
        return Object.keys(value);
    }
}