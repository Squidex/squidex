/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class RuleConfigured {
    constructor(
        public readonly trigger: any,
        public readonly action: any,
    ) {
    }
}
