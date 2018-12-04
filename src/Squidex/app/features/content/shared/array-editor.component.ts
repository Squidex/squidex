/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AbstractControl, FormArray } from '@angular/forms';

import {
    AppLanguageDto,
    EditContentForm,
    ImmutableArray,
    RootFieldDto
} from '@app/shared';

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
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

    public isHidden = false;

    public hide(hide: boolean) {
        this.isHidden = hide;
    }

    public removeItem(index: number) {
        this.form.removeArrayItem(this.field, this.language, index);
    }

    public addItem(value: {}) {
        this.form.insertArrayItem(this.field, this.language, value);
    }

    public sort(controls: AbstractControl[]) {
        for (let i = 0; i < controls.length; i++) {
            this.arrayControl.setControl(i, controls[i]);
        }
    }
}