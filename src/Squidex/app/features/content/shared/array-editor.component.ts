/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';

import {
    AppLanguageDto,
    EditContentForm,
    RootFieldDto,
    StatefulComponent
} from '@app/shared';

interface State {
    isHidden: boolean;
}

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayEditorComponent extends StatefulComponent<State> {
    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    @Input()
    public arrayControl: FormArray;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            isHidden: false
        });
    }

    public hide(isHidden: boolean) {
        this.next(s => ({ ...s, isHidden }));
    }

    public removeItem(index: number) {
        this.form.removeArrayItem(this.field, this.language, index);
    }

    public addItem(value?: FormGroup) {
        this.form.insertArrayItem(this.field, this.language, value);
    }

    public sort(controls: AbstractControl[]) {
        for (let i = 0; i < controls.length; i++) {
            this.arrayControl.setControl(i, controls[i]);
        }
    }

    public move(control: AbstractControl, index: number) {
        let controls = [...this.arrayControl.controls];

        controls.splice(controls.indexOf(control), 1);
        controls.splice(index, 0, control);

        this.sort(controls);
    }
}