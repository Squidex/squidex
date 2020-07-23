/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormControl } from '@angular/forms';
import { AbstractContentForm, AppLanguageDto, EditContentForm, FieldDto, MathHelper, RootFieldDto, Types } from '@app/shared';

@Component({
    selector: 'sqx-field-editor',
    styleUrls: ['./field-editor.component.scss'],
    templateUrl: './field-editor.component.html'
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
    public displaySuffix: string;

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    public get field() {
        return this.formModel.field;
    }

    public get arrayControl() {
        return this.formModel.form as FormArray;
    }

    public get editorControl() {
        return this.formModel.form as FormControl;
    }

    public get rootField() {
        return this.formModel.field as RootFieldDto;
    }

    public uniqueId = MathHelper.guid();

    public ngOnChanges(changes: SimpleChanges) {
        const previousControl = changes['control']?.previousValue;

        if (previousControl && Types.isFunction(previousControl['_clearChangeFns'])) {
            previousControl['_clearChangeFns']();
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
}