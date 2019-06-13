/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import {
    hasAnyLink,
    Resource,
    ResourceLinks
} from '@app/framework/internal';

@Pipe({
    name: 'sqxHasLink',
    pure: true
})
export class HasLinkPipe implements PipeTransform {
    public transform(value: Resource | ResourceLinks,  ...rels: string[]) {
        return hasAnyLink(value, ...rels);
    }
}

@Pipe({
    name: 'sqxHasNoLink',
    pure: true
})
export class HasNoLinkPipe implements PipeTransform {
    public transform(value: Resource | ResourceLinks,  ...rels: string[]) {
        return !hasAnyLink(value, ...rels);
    }
}