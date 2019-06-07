/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

 export interface Resource {
    _links?: { [rel: string]: ResourceLink };
 }

 export type ResourceLinks = { [rel: string]: ResourceLink };
 export type ResourceLink = { href: string; method: ResourceMethod; };

 export function withLinks<T extends Resource>(value: T, source: Resource) {
     value._links = source._links;

     return value;
 }

 export type ResourceMethod =
    'get' |
    'post' |
    'put' |
    'delete';