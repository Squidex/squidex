/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxSchemaScriptName',
    pure: true,
})
export class SchemaScriptNamePipe implements PipeTransform {
    public transform(value: string) {
        if (value === 'queryPre') {
            return 'Prepare Query';
        } else {
            return value.substring(0, 1).toUpperCase() + value.substring(1);
        }
    }
}