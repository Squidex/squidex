/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-hint',
    template: `
        <small class="text-muted form-text mt-{{marginTop}} mb-{{marginBottom}} {{class}}">
            <ng-content></ng-content>
        </small>
    `,
    styles: [`
        :host {
            display: block;
            margin-top: 0;
            margin-bottom: .5rem
        }

        :host::last-child {
            margin-bottom: 0;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormHintComponent {
    @Input()
    public class: string;

    @Input()
    public marginTop = 0;

    @Input()
    public marginBottom = 0;
}