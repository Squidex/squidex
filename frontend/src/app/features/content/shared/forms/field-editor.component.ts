/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf, NgSwitch, NgSwitchCase } from '@angular/common';
import { booleanAttribute, Component, ElementRef, EventEmitter, Input, numberAttribute, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AbstractContentForm, AppLanguageDto, ChatDialogComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, ConfirmClickDirective, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, EditContentForm, FieldDto, FormHintComponent, GeolocationEditorComponent, hasNoValue$, IndeterminateValueDirective, MarkdownDirective, MathHelper, ModalDirective, RadioGroupComponent, ReferenceInputComponent, RichEditorComponent, StarsComponent, TagEditorComponent, ToggleComponent, TooltipDirective, TransformInputDirective, TypedSimpleChanges, Types } from '@app/shared';
import { ReferenceDropdownComponent } from '../references/reference-dropdown.component';
import { ReferencesCheckboxesComponent } from '../references/references-checkboxes.component';
import { ReferencesEditorComponent } from '../references/references-editor.component';
import { ReferencesTagsComponent } from '../references/references-tags.component';
import { ArrayEditorComponent } from './array-editor.component';
import { AssetsEditorComponent } from './assets-editor.component';
import { ComponentComponent } from './component.component';
import { IFrameEditorComponent } from './iframe-editor.component';
import { StockPhotoEditorComponent } from './stock-photo-editor.component';

@Component({
    standalone: true,
    selector: 'sqx-field-editor',
    styleUrls: ['./field-editor.component.scss'],
    templateUrl: './field-editor.component.html',
    imports: [
        ArrayEditorComponent,
        AssetsEditorComponent,
        AsyncPipe,
        ChatDialogComponent,
        CheckboxGroupComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        ComponentComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        DateTimeEditorComponent,
        FormHintComponent,
        FormsModule,
        GeolocationEditorComponent,
        IFrameEditorComponent,
        IndeterminateValueDirective,
        MarkdownDirective,
        ModalDirective,
        NgFor,
        NgIf,
        NgSwitch,
        NgSwitchCase,
        RadioGroupComponent,
        ReactiveFormsModule,
        ReferenceDropdownComponent,
        ReferenceInputComponent,
        ReferencesCheckboxesComponent,
        ReferencesEditorComponent,
        ReferencesTagsComponent,
        RichEditorComponent,
        StarsComponent,
        StockPhotoEditorComponent,
        TagEditorComponent,
        ToggleComponent,
        TooltipDirective,
        TransformInputDirective,
    ],
})
export class FieldEditorComponent {
    public readonly uniqueId = MathHelper.guid();

    @Output()
    public expandedChange = new EventEmitter();

    @Input({ required: true })
    public hasChatBot!: boolean;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: AbstractContentForm<FieldDto, AbstractControl>;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ transform: numberAttribute })
    public index: number | null | undefined;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input({ transform: booleanAttribute })
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
        if (value) {
            this.formModel.setValue(value);
        }

        this.chatDialog.hide();
    }
}
