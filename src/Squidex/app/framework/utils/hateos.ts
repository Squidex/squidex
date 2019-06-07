/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export interface Resource {
    readonly _links: { [rel: string]: ResourceLink };
}

export type ResourceLinks = { [rel: string]: ResourceLink };
export type ResourceLink = { href: string; method: ResourceMethod; };

export function withLinks<T extends Resource>(value: T, source: Resource) {
    if (value._links && source._links) {
        for (let key in source._links) {
            if (source._links.hasOwnProperty(key)) {
                value._links[key] = source._links[key];
            }
        }

        Object.freeze(value._links);
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