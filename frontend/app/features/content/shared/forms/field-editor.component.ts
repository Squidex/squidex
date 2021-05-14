/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, FormControl } from '@angular/forms';
import { AbstractContentForm, AppLanguageDto, EditContentForm, FieldDto, MathHelper, RootFieldDto, Types } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-field-editor',
    styleUrls: ['./field-editor.component.scss'],
    templateUrl: './field-editor.component.html',
})
export class FieldEditorComponent implements OnChanges {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: AbstractContentForm<FieldDto, AbstractControl>;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public index: number;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public displaySuffix: string;

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    public isEmpty: Observable<boolean>;

    public get field() {
        return this.formModel.field;
    }

    public get editorControl() {
        return this.formModel.form as FormControl;
    }

    public get rootField() {
        return this.formModel.field as RootFieldDto;
    }

    public uniqueId = MathHelper.guid();

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            const previousControl: AbstractContentForm<FieldDto, AbstractControl> = changes['formModel'].previousValue;

            if (previousControl && Types.isFunction(previousControl.form['_clearChangeFns'])) {
                previousControl.form['_clearChangeFns']();
            }

            this.isEmpty = this.formModel.form.valueChanges.pipe(map(x => Types.isUndefined(x) || Types.isNull(x)));
        }
    }

    public reset() {
        if (this.editor) {
            const nativeElement = this.editor.nativeElement;

            if (nativeElement && Types.isFunction(nativeElement['reset'])) {
                nativeElement['reset']();
            }

            if (this.editor && Types.isFunction(this.editor['reset'])) {
                this.editor['reset']();
            }
        }
    }

    public unset() {
        this.formModel.unset();
    }
}
