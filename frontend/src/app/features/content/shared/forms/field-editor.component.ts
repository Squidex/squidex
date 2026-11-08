/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, ElementRef, inject, input, numberAttribute, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AbstractContentForm, AnnotationCreate, AnnotationsSelected, AnyFieldDto, AppLanguageDto, ChatDialogComponent, CheckboxComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, CommentSelected, CommentsState, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, EditContentForm, FieldSelected, FormHintComponent, GeolocationEditorComponent, HTTP, isValidValue, MarkdownDirective, MathHelper, MenuComponent, MenuItemComponent, MessageBus, ModalDirective, RadioGroupComponent, ReferenceInputComponent, ReferencesFieldPropertiesDto, RichEditorComponent, SignalSelectDirective, SignalSelectMultipleDirective, StarsComponent, TagEditorComponent, TextInputDirective, ToggleComponent, TooltipDirective, Types } from '@app/shared';
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
        ChatDialogComponent,
        CheckboxComponent,
        CheckboxGroupComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        ComponentComponent,
        ControlErrorsComponent,
        DateTimeEditorComponent,
        FormField,
        FormHintComponent,
        FormsModule,
        GeolocationEditorComponent,
        IFrameEditorComponent,
        MarkdownDirective,
        MenuComponent,
        MenuItemComponent,
        ModalDirective,
        RadioGroupComponent,
        ReferenceDropdownComponent,
        ReferenceInputComponent,
        ReferencesCheckboxesComponent,
        ReferencesEditorComponent,
        ReferencesRadioButtonsComponent,
        ReferencesTagsComponent,
        RichEditorComponent,
        SignalSelectDirective,
        SignalSelectMultipleDirective,
        StarsComponent,
        StockPhotoEditorComponent,
        TagEditorComponent,
        TextInputDirective,
        ToggleComponent,
        TooltipDirective,
        UserInfoEditorComponent,
    ],
})
export class FieldEditorComponent {
    private readonly messageBus = inject(MessageBus);

    /** Emits when the editor wants to change the expanded state. */
    public readonly expandedChange = output();

    /** The comments to show annotations for, if annotations are supported. */
    public readonly comments = input<CommentsState | null>();

    /** Enables the AI features of the editor. */
    public readonly hasChatBot = input.required<boolean>();

    /** The content form the field belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the field, 0 for root fields. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The form model of the field to edit. */
    public readonly formModel = input.required<AbstractContentForm<AnyFieldDto>>();

    /** The language of the field value. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Hides the editor and disables the actions. */
    public readonly isCollapsed = input.required<boolean>();

    /** The index of the field when it is part of an array. */
    public readonly index = input<number | null | undefined, unknown>(undefined, { transform: numberAttribute });

    /** Renders the editor next to the editor of another version. */
    public readonly isComparing = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** The text to show after the field name, for example the language. */
    public readonly displaySuffix = input('');

    protected readonly root = viewChild<ElementRef<HTMLElement>>('root');

    protected readonly editor = viewChild<any>('editor');

    protected readonly isSelected = signal(false);

    protected readonly isExpanded = signal(false);

    protected readonly field = computed(() => {
        return this.formModel().field;
    });

    protected readonly fieldForm = computed<any>(() => {
        return this.formModel().form;
    });

    protected readonly isString = computed(() => {
        return this.field().properties.fieldType === 'String';
    });

    protected readonly hasComments = computed(() => {
        return this.field().properties.hasComments;
    });

    protected readonly schemaIds = computed(() => {
        const properties = this.field().properties;

        return Types.is(properties, ReferencesFieldPropertiesDto) ? properties.schemaIds : undefined;
    });

    protected readonly isDisabled = computed(() => {
        return this.formModel().disabled();
    });

    protected readonly isEmpty = computed(() => {
        return !isValidValue(this.formModel().value());
    });

    protected readonly annotations = computed(() => {
        return this.comments()?.getAnnotations(this.formModel().path);
    }, { equal: Types.equals });

    protected readonly uniqueId = MathHelper.guid();

    protected readonly chatDialog = new DialogModel();

    constructor() {
        this.messageBus.on(CommentSelected, message => {
            const isSelected = message.editorId.indexOf(this.formModel().path) === 0;

            if (isSelected) {
                this.root()?.nativeElement.scrollIntoView();
            }

            this.isSelected.set(isSelected);
        });
    }

    public reset() {
        const editor = this.editor();
        if (!editor) {
            return;
        }

        const nativeElement = editor.nativeElement;

        if (nativeElement && Types.isFunction(nativeElement['reset'])) {
            nativeElement['reset']();
        }

        if (Types.isFunction(editor['reset'])) {
            editor['reset']();
        }
    }

    protected toggleExpanded() {
        this.isExpanded.update(x => !x);
    }

    protected select() {
        this.messageBus.emit(new FieldSelected(this.formModel().path));
    }

    protected annotationCreate(annotation?: AnnotationSelection) {
        this.messageBus.emit(new AnnotationCreate(this.formModel().path, annotation));
    }

    protected annotationsSelect(annotation: ReadonlyArray<string>) {
        this.messageBus.emit(new AnnotationsSelected(annotation));
    }

    protected annotationsUpdate(annotations: ReadonlyArray<Annotation>) {
        this.comments()?.updateAnnotations(this.formModel().path, annotations);
    }

    protected unset() {
        this.formModel().unset();
    }

    protected setValue(content: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (Types.isString(content)) {
            this.formModel().setValue(content);
        }
    }
}
