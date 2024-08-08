/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, ElementRef, EventEmitter, Input, numberAttribute, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { AbstractContentForm, AnnotationCreate, AnnotationsSelect, AppLanguageDto, ChatDialogComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, CommentsState, ConfirmClickDirective, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, disabled$, EditContentForm, FieldDto, FormHintComponent, GeolocationEditorComponent, hasNoValue$, HTTP, IndeterminateValueDirective, MarkdownDirective, MathHelper, MessageBus, ModalDirective, RadioGroupComponent, ReferenceInputComponent, RichEditorComponent, StarsComponent, TagEditorComponent, ToggleComponent, TooltipDirective, TransformInputDirective, TypedSimpleChanges, Types } from '@app/shared';
import { ReferenceDropdownComponent } from '../references/reference-dropdown.component';
import { ReferencesCheckboxesComponent } from '../references/references-checkboxes.component';
import { ReferencesEditorComponent } from '../references/references-editor.component';
import { ReferencesRadioButtonsComponent } from '../references/references-radio-buttons.component';
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
        RadioGroupComponent,
        ReactiveFormsModule,
        ReferenceDropdownComponent,
        ReferenceInputComponent,
        ReferencesCheckboxesComponent,
        ReferencesEditorComponent,
        ReferencesRadioButtonsComponent,
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

    @Input()
    public comments?: CommentsState | null;

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

    @Input()
    public displaySuffix = '';

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public isEmpty?: Observable<boolean>;
    public isExpanded = false;
    public isDisabled?: Observable<boolean>;

    public chatDialog = new DialogModel();

    public annotations?: Observable<ReadonlyArray<Annotation>>;

    public get field() {
        return this.formModel.field;
    }

    public get fieldForm() {
        return this.formModel.form;
    }

    public get isString() {
        return this.field?.properties.fieldType === 'String';
    }

    constructor(
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.isEmpty = hasNoValue$(this.formModel.form);
            this.isDisabled = disabled$(this.formModel.form);
        }

        if (changes.formModel || changes.comments) {
            this.annotations = this.comments?.getAnnotations(this.formModel.fieldPath);
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

    public annotationCreate(annotation: AnnotationSelection) {
        this.messageBus.emit(new AnnotationCreate(this.formModel.fieldPath, annotation));
    }

    public annotationsSelect(annotation: ReadonlyArray<string>) {
        this.messageBus.emit(new AnnotationsSelect(annotation));
    }

    public annotationsUpdate(annotations: ReadonlyArray<Annotation>) {
        this.comments?.updateAnnotations(this.formModel.fieldPath, annotations);
    }

    public unset() {
        this.formModel.unset();
    }

    public setValue(content: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (Types.isString(content)) {
            this.formModel.setValue(content);
        }
    }
}
