/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DynamicFlowStepDefinitionDto } from '@app/shared';

export type FlowStepRemove = { id: string; parentId?: string | null; branchIndex: number };

export type FlowStepAdd = { afterId?: string; parentId?: string | null; branchIndex: number };

export type FlowStepUpdate = { id: string; values: DynamicFlowStepDefinitionDto };

export type RuleElement<T> = { type: string; metadata: T };

export function getSortedElements<T extends { title?: string }>(source: Record<string, T>): ReadonlyArray<{ type: string; metadata: T }> {
    const items =
        Object.entries(source)
            .map(([type, metadata]) => ({ type, metadata }));

    return items.sortedByString(x => x.metadata.title!);
}