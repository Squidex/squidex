/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, ElementRef, input, numberAttribute, output, signal, viewChild } from '@angular/core';
import { AbstractControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AbstractContentForm, AnnotationCreate, AnnotationsSelected, AnyFieldDto, AppLanguageDto, ChatDialogComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, CommentSelected, CommentsState, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, disabled$, EditContentForm, FieldSelected, FormHintComponent, GeolocationEditorComponent, hasNoValue$, HTTP, IndeterminateValueDirective, MarkdownDirective, MathHelper, MenuComponent, MenuItemComponent, MessageBus, ModalDirective, RadioGroupComponent, ReferenceInputComponent, ReferencesFieldPropertiesDto, RichEditorComponent, StarsComponent, TagEditorComponent, ToggleComponent, TooltipDirective, TransformInputDirective, Types } from '@app/shared';
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
    changeDetection: ChangeDetectionStrategy.OnPush,
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
        TooltipDirective,
        TransformInputDirective,
        UserInfoEditorComponent,
    ],
})
export class FieldEditorComponent {
    private readonly root = viewChild<ElementRef<HTMLElement>>('root');
    private readonly editor = viewChild<any>('editor');

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
    public readonly formModel = input.required<AbstractContentForm<AnyFieldDto, AbstractControl>>();

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

    public readonly uniqueId = MathHelper.guid();

    public readonly isExpanded = signal(false);
    public readonly isSelected = signal(false);

    public readonly chatDialog = new DialogModel();

    public readonly field = computed(() => {
        return this.formModel().field;
    });

    public readonly fieldForm = computed(() => {
        return this.formModel().form;
    });

    public readonly isString = computed(() => {
        return this.field().properties.fieldType === 'String';
    });

    public readonly hasComments = computed(() => {
        return this.field().properties.hasComments;
    });

    public readonly schemaIds = computed(() => {
        const properties = this.field().properties;

        return Types.is(properties, ReferencesFieldPropertiesDto) ? properties.schemaIds : undefined;
    });

    public readonly isDisabled = computed(() => {
        return disabled$(this.formModel().form);
    });

    public readonly isEmpty = computed(() => {
        return hasNoValue$(this.formModel().form);
    });

    public readonly annotations = computed(() => {
        return this.comments()?.getAnnotations(this.formModel().path);
    });

    constructor(
        private readonly messageBus: MessageBus,
    ) {
        effect(onCleanup => {
            const subscription =
                this.messageBus.of(CommentSelected)
                    .subscribe(message => {
                        const isSelected = message.editorId.indexOf(this.formModel().path) === 0;

                        if (isSelected) {
                            this.root()?.nativeElement.scrollIntoView();
                        }

                        this.isSelected.set(isSelected);
                    });

            onCleanup(() => subscription.unsubscribe());
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

    public toggleExpanded() {
        this.isExpanded.update(x => !x);
    }

    public select() {
        this.messageBus.emit(new FieldSelected(this.formModel().path));
    }

    public annotationCreate(annotation?: AnnotationSelection) {
        this.messageBus.emit(new AnnotationCreate(this.formModel().path, annotation));
    }

    public annotationsSelect(annotation: ReadonlyArray<string>) {
        this.messageBus.emit(new AnnotationsSelected(annotation));
    }

    public annotationsUpdate(annotations: ReadonlyArray<Annotation>) {
        this.comments()?.updateAnnotations(this.formModel().path, annotations);
    }

    public unset() {
        this.formModel().unset();
    }

    public setValue(content: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (Types.isString(content)) {
            this.formModel().setValue(content);
        }
    }
}
