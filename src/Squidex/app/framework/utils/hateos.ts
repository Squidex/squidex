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

export function withLinks<T extends Resource>(value: T, source: Resource) {
    if (value._links && source._links) {
        if (!value._links) {
            value._links = {};
        }

        for (let key in source._links) {
            if (source._links.hasOwnProperty(key)) {
                value._links[key] = source._links[key];
            }
        }

        Object.freeze(value._links);
    }

    if (source._meta) {
        if (!value._meta) {
            value._meta = {};
        }

        for (let key in source._meta) {
            if (source._meta.hasOwnProperty(key)) {
                value._meta[key] = source._meta[key];
            }
        }

        Object.freeze(value._meta);
    }

    return value;
}

export function hasLink(value: Resource | ResourceLinks, rel: string): boolean {
    const link = getLink(value, rel);

    return !!(link && link.method && link.href);
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