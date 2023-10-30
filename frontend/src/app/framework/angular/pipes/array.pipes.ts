/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxReverse',
    pure: true,
})
export class ReversePipe implements PipeTransform {
    public transform<T>(value: ReadonlyArray<T>) {
        return value.slice().reverse();
    }
}