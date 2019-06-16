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
export type ResourceLink = { href: string; method: ResourceMethod; };

export type Metadata = { [rel: string]: string };

function hasLink(value: Resource | ResourceLinks, rel: string): boolean {
    const link = getLink(value, rel);

    return !!(link && link.method && link.href);
}

export function hasAnyLink(value: Resource | ResourceLinks,  ...rels: string[]) {
    for (let rel of rels) {
        if (hasLink(value, rel)) {
            return true;
        }
    }

    return false;
}

export function getLink(value: Resource | ResourceLinks, rel: string): ResourceLink {
    return value ? (value._links ? value._links[rel] : value[rel]) : undefined;
}

export type ResourceMethod =
    'GET' |
    'DELETE' |
    'PATCH' |
    'POST' |
    'PUT';