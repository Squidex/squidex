/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'sqx-form-hint',
    template: `
        <small class="text-muted form-text">
            <ng-content></ng-content>
        </small>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormHintComponent {}