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
export type ResourceLink = { href: string; method: ResourceMethod; metadata?: string };

export type Metadata = { [rel: string]: string };

export function getLinkUrl(value: Resource | ResourceLinks, ...rels: ReadonlyArray<string>) {
    if (!value) {
        return false;
    }

    const links = value._links || value;

    for (const rel of rels) {
        const link = links[rel];

        if (link && link.method && link.href) {
            return link.href;
        }
    }

    return undefined;
}

export function hasAnyLink(value: Resource | ResourceLinks, ...rels: ReadonlyArray<string>) {
    return !!getLinkUrl(value, ...rels);
}

export type ResourceMethod =
    'GET' |
    'DELETE' |
    'PATCH' |
    'POST' |
    'PUT';
