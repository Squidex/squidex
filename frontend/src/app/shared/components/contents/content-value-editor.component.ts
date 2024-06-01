/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { IndeterminateValueDirective, StarsComponent, ToggleComponent, TransformInputDirective } from '@app/framework';
import { FieldDto, MathHelper } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-content-value-editor',
    styleUrls: ['./content-value-editor.component.scss'],
    templateUrl: './content-value-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        IndeterminateValueDirective,
        ReactiveFormsModule,
        StarsComponent,
        ToggleComponent,
        TransformInputDirective,
    ],
})
export class ContentValueEditorComponent {
    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public form!: UntypedFormGroup;

    public readonly uniqueId = MathHelper.guid();
}
