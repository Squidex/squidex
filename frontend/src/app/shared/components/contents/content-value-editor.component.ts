/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgSwitch, NgSwitchCase } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { IndeterminateValueDirective, StarsComponent, ToggleComponent, TransformInputDirective } from '@app/framework';
import { FieldDto, MathHelper } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value-editor',
    styleUrls: ['./content-value-editor.component.scss'],
    templateUrl: './content-value-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        NgSwitch,
        NgSwitchCase,
        StarsComponent,
        NgFor,
        TransformInputDirective,
        ToggleComponent,
        IndeterminateValueDirective,
    ],
})
export class ContentValueEditorComponent {
    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public form!: UntypedFormGroup;

    public readonly uniqueId = MathHelper.guid();
}
