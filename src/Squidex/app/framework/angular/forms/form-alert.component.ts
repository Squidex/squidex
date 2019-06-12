/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-alert',
    styles: [`
        :host {
            display: block;
            min-width: 100%;
            max-width: 100%;
        }
    `],
    template: `
        <div class="alert alert-hint mt-{{marginTop}} mb-{{marginBottom}} {{class}}">
            <i class="icon-info-outline"></i> <ng-content></ng-content>
        </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormAlertComponent {
    @Input()
    public class: string;

    @Input()
    public marginTop = 2;

    @Input()
    public marginBottom = 4;
}