/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeComponent, ControlErrorsComponent, FormHintComponent, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-asset-changed-trigger',
    styleUrls: ['./asset-changed-trigger.component.scss'],
    templateUrl: './asset-changed-trigger.component.html',
    imports: [
        CodeComponent,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class AssetChangedTriggerComponent {
    @Input({ required: true })
    public triggerForm!: TriggerForm;
}
