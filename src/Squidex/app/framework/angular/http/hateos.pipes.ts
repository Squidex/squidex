/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import {
    hasLink,
    Resource,
    ResourceLinks
} from '@app/framework/internal';

@Pipe({
    name: 'sqxHasLink',
    pure: true
})
export class HasLinkPipe implements PipeTransform {
    public transform(value: Resource | ResourceLinks,  ...rels: string[]) {
        for (let rel of rels) {
            if (hasLink(value, rel)) {
                return true;
            }
        }

        return false;
    }
}