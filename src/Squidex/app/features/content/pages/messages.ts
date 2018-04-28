/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Version } from '@app/shared';

export class ContentVersionSelected {
    constructor(
        public readonly version: Version
    ) {
    }
}