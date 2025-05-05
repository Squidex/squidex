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

@Pipe({
    name: 'sqxKeys',
    pure: true,
    standalone: true,
})
export class KeysPipe implements PipeTransform {
    public transform(value: any): any {
        return Object.keys(value).sort();
    }
}

@Pipe({
    name: 'sqxEntries',
    pure: true,
    standalone: true,
})
export class EntriesPipe implements PipeTransform {
    public transform<T>(value: Record<string, T>, sort?: string): ReadonlyArray<{ key: string; value: T }> {
        const result = Object.entries(value).map(([key, value]) => ({ key, value }));

        if (sort) {
            result.sortByString(x => (x.value as any)?.[sort]?.toString() || '');
        }

        return result;
    }
}
