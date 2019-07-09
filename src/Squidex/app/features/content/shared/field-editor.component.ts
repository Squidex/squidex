/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';

import {
    AppLanguageDto,
    EditContentForm,
    FieldDto,
    MathHelper
} from '@app/shared';

@Component({
    selector: 'sqx-field-editor',
    styleUrls: ['./field-editor.component.scss'],
    templateUrl: './field-editor.component.html'
})
export class FieldEditorComponent {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public field: FieldDto;

    @Input()
    public control: FormControl;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    @Input()
    public isCompact = false;

    @Input()
    public displaySuffix: string;

    public uniqueId = MathHelper.guid();
}