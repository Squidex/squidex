/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

declare module 'graphiql';
declare module 'pikaday/pikaday';
declare module 'progressbar.js';

declare module 'sortablejs' {
    export default class Sortable {
        public destroy(): any;

        public static create(element: any, options: any): Sortable;
    }
}
