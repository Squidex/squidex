/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxFilterOperator',
    pure: true,
})
export class FilterOperatorPipe implements PipeTransform {
    public transform(value: any) {
        if (!value) {
            return null;
        }

        return `common.queryOperators.${value}`;
    }
}