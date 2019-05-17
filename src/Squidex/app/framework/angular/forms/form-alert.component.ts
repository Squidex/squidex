/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-alert',
    template: `
        <div class="alert alert-hint mt-2 {{class}}">
            <i class="icon-info-outline"></i> <ng-content></ng-content>
        </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormAlertComponent {
    @Input()
    public class: string;
}