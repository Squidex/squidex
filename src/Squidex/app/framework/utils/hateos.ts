
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export interface Resource {
    _links: ResourceLinks;

    _meta?: Metadata;
}

export type ResourceLinks = { [rel: string]: ResourceLink };
export type ResourceLink = { href: string; method: ResourceMethod; metadata?: string; };

export type Metadata = { [rel: string]: string };

export function hasAnyLink(value: Resource | ResourceLinks,  ...rels: string[]) {
    if (!value) {
        return false;
    }

    const links = value._links || value;

    for (let rel of rels) {
        const link = links[rel];

        if (link && link.method && link.href) {
            return true;
        }
    }

    return false;
}

export type ResourceMethod =
    'GET' |
    'DELETE' |
    'PATCH' |
    'POST' |
    'PUT';