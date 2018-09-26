/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormArray } from '@angular/forms';

import {
    AppLanguageDto,
    EditContentForm,
    ImmutableArray,
    RootFieldDto
} from '@app/shared';

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html'
})
export class ArrayEditorComponent {
    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Input()
    public arrayControl: FormArray;

    public removeItem(index: number) {
        this.form.removeArrayItem(this.field, this.language, index);
    }

    public addItem() {
        this.form.insertArrayItem(this.field, this.language);
    }
}