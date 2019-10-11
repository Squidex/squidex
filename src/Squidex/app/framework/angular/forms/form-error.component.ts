/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-error',
    template: `
        <ng-container *ngIf="error">
            <div class="form-alert form-alert-error" [innerHTML]="error"></div>
        </ng-container>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormErrorComponent {
    @Input()
    public error: string | null | undefined;
}