/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { CheckboxComponent, SignalSelectDirective, StarsComponent, TextInputDirective, ToggleComponent } from '@app/framework';
import { FieldDto, MathHelper } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value-editor',
    styleUrls: ['./content-value-editor.component.scss'],
    templateUrl: './content-value-editor.component.html',
    imports: [
        CheckboxComponent,
        FormField,
        FormsModule,
        SignalSelectDirective,
        StarsComponent,
        TextInputDirective,
        ToggleComponent,
    ],
})
export class ContentValueEditorComponent {
    /** The field to render an editor for. */
    public readonly field = input.required<FieldDto>();

    /** The form group that contains a control with the name of the field. */
    public readonly form = input.required<FieldTree<Record<string, any>>>();

    protected readonly uniqueId = MathHelper.guid();
}
