/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { IndeterminateValueDirective, StarsComponent, ToggleComponent, TransformInputDirective } from '@app/framework';
import { FieldDto, MathHelper } from '@app/shared/internal';

@Component({
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
    /** The field to render an editor for. */
    public readonly field = input.required<FieldDto>();

    /** The form group that contains a control with the name of the field. */
    public readonly form = input.required<UntypedFormGroup>();

    protected readonly uniqueId = MathHelper.guid();
}
