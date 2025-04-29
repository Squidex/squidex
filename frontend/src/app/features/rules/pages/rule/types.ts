/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export type FlowStepRemove = { id: string; parentId?: string; branchIndex: number };

export type FlowStepAdd = { afterId?: string; parentId?: string; branchIndex: number };