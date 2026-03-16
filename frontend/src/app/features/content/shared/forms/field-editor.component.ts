/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, ElementRef, EventEmitter, Input, numberAttribute, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { AbstractContentForm, AnnotationCreate, AnnotationsSelected, AnyFieldDto, AppLanguageDto, ChatDialogComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, CommentSelected, CommentsState, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, disabled$, EditContentForm, FieldSelected, FormHintComponent, GeolocationEditorComponent, hasNoValue$, HTTP, IndeterminateValueDirective, MarkdownDirective, MathHelper, MenuComponent, MenuItemComponent, MessageBus, ModalDirective, RadioGroupComponent, ReferenceInputComponent, ReferencesFieldPropertiesDto, RichEditorComponent, StarsComponent, Subscriptions, TagEditorComponent, ToggleComponent, TransformInputDirective, TypedSimpleChanges, Types } from '@app/shared';
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
import { UserInfoEditorComponent } from './user-info-editor.component';

@Component({
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
        ControlErrorsComponent,
        DateTimeEditorComponent,
        FormHintComponent,
        FormsModule,
        GeolocationEditorComponent,
        IFrameEditorComponent,
        IndeterminateValueDirective,
        MarkdownDirective,
        MenuComponent,
        MenuItemComponent,
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
        TransformInputDirective,
        UserInfoEditorComponent,
    ],
})
export class FieldEditorComponent {
    private readonly subscriptions = new Subscriptions();

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
    public formModel!: AbstractContentForm<AnyFieldDto, AbstractControl>;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ required: true })
    public isCollapsed = false;

    @Input({ transform: numberAttribute })
    public index: number | null | undefined;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input()
    public displaySuffix = '';

    @ViewChild('root', { static: false })
    public root!: ElementRef<HTMLElement>;

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public readonly uniqueId = MathHelper.guid();

    public isDisabled!: Observable<boolean>;
    public isEmpty!: Observable<boolean>;
    public isExpanded = false;
    public isSelected = new BehaviorSubject<boolean>(false);
    public chatDialog = new DialogModel();
    public annotations?: Observable<ReadonlyArray<Annotation>>;

    public get field() {
        return this.formModel.field;
    }

    public get fieldForm() {
        return this.formModel.form;
    }

    public get isString() {
        return this.field.properties.fieldType === 'String';
    }

    public get hasComments() {
        return this.field.properties.hasComments;
    }

    public get schemaIds() {
        return Types.is(this.field.properties, ReferencesFieldPropertiesDto) ? this.field.properties.schemaIds : undefined;
    }

    constructor(
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.messageBus.of(CommentSelected)
                .subscribe(message => {
                    const isSelected = message.editorId.indexOf(this.formModel.path) === 0;

                    if (isSelected) {
                        this.root.nativeElement.scrollIntoView();
                    }

                    if (isSelected !== this.isSelected.value) {
                        this.isSelected.next(isSelected);
                    }
                }));
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.isDisabled = disabled$(this.formModel.form);
            this.isEmpty = hasNoValue$(this.formModel.form);
        }

        if (changes.formModel || changes.comments) {
            this.annotations = this.comments?.getAnnotations(this.formModel.path);
        }
    }

    public reset() {
        const editor = this.editor as any;
        if (!editor) {
            return;
        }

        const nativeElement = this.editor.nativeElement;

        if (nativeElement && Types.isFunction(nativeElement['reset'])) {
            nativeElement['reset']();
        }

        if (this.editor && Types.isFunction(editor['reset'])) {
            editor['reset']();
        }
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }

    public select() {
        this.messageBus.emit(new FieldSelected(this.formModel.path));
    }

    public annotationCreate(annotation?: AnnotationSelection) {
        this.messageBus.emit(new AnnotationCreate(this.formModel.path, annotation));
    }

    public annotationsSelect(annotation: ReadonlyArray<string>) {
        this.messageBus.emit(new AnnotationsSelected(annotation));
    }

    public annotationsUpdate(annotations: ReadonlyArray<Annotation>) {
        this.comments?.updateAnnotations(this.formModel.path, annotations);
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
