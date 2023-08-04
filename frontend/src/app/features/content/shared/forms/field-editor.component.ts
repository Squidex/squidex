/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { AbstractContentForm, AppLanguageDto, DialogModel, EditContentForm, FieldDto, hasNoValue$, MathHelper, TypedSimpleChanges, Types } from '@app/shared';

@Component({
    selector: 'sqx-field-editor',
    styleUrls: ['./field-editor.component.scss'],
    templateUrl: './field-editor.component.html',
})
export class FieldEditorComponent {
    public readonly uniqueId = MathHelper.guid();

    @Output()
    public expandedChange = new EventEmitter();

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: AbstractContentForm<FieldDto, AbstractControl>;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public index: number | null | undefined;

    @Input({ required: true })
    public isComparing = false;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public displaySuffix = '';

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public isEmpty?: Observable<boolean>;
    public isExpanded = false;

    public chatDialog = new DialogModel();

    public get field() {
        return this.formModel.field;
    }

    public get fieldForm() {
        return this.formModel.form;
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.isEmpty = hasNoValue$(this.formModel.form);
        }
    }

    public reset() {
        const editor = this.editor as any;

        if (editor) {
            const nativeElement = this.editor.nativeElement;

            if (nativeElement && Types.isFunction(nativeElement['reset'])) {
                nativeElement['reset']();
            }

            if (this.editor && Types.isFunction(editor['reset'])) {
                editor['reset']();
            }
        }
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }

    public unset() {
        this.formModel.unset();
    }

    public setValue(value: any) {
        this.formModel.setValue(value);

        this.chatDialog.hide();
    }
}
