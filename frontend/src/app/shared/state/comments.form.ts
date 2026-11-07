/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SignalForm } from '@app/framework';

export type UpsertCommentFormModel = { text: string };

export class UpsertCommentForm extends SignalForm<UpsertCommentFormModel, UpsertCommentFormModel> {
    constructor() {
        super({ text: '' });
    }
}
