/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DynamicFlowDefinitionDto, RuleTriggerDto } from '@app/shared';

export class RuleConfigured {
    constructor(
        public readonly trigger: RuleTriggerDto,
        public readonly flow: DynamicFlowDefinitionDto,
    ) {}
}
