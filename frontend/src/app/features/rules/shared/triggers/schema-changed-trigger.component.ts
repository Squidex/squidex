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
    selector: 'sqx-schema-changed-trigger',
    styleUrls: ['./schema-changed-trigger.component.scss'],
    templateUrl: './schema-changed-trigger.component.html',
    imports: [
        CodeComponent,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class SchemaChangedTriggerComponent {
    @Input({ required: true })
    public triggerForm!: TriggerForm;
}
