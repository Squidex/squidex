/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxJoin',
    pure: true,
    standalone: true,
})
export class JoinPipe implements PipeTransform {
    public transform(value: ReadonlyArray<string>) {
        return value?.join(', ') || '';
    }
}