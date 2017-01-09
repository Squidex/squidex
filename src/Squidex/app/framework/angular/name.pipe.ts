/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Pipe } from '@angular/core';

import { StringHelper } from './../utils/string-helper';

@Pipe({
    name: 'displayName'
})
export class DisplayNamePipe {
    public transform(value: any, field1 = 'label', field2  = 'name'): any {
        if (!value) {
            return '';
        }

        return StringHelper.firstNonEmpty(value[field1], value[field2]);
    }
}