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
import { AbstractContentForm, AnnotationCreate, AnnotationsSelect, AnyFieldDto, AppLanguageDto, ChatDialogComponent, CheckboxGroupComponent, CodeEditorComponent, ColorPickerComponent, CommentsState, ControlErrorsComponent, DateTimeEditorComponent, DialogModel, disabled$, EditContentForm, FormHintComponent, GeolocationEditorComponent, hasNoValue$, HTTP, IndeterminateValueDirective, isValidValue, MarkdownDirective, MathHelper, MenuComponent, MenuItem, MessageBus, ModalDirective, RadioGroupComponent, ReferenceInputComponent, ReferencesFieldPropertiesDto, RichEditorComponent, StarsComponent, Subscriptions, TagEditorComponent, ToggleComponent, TransformInputDirective, TypedSimpleChanges, Types } from '@app/shared';
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
        ModalDirective,
        MenuComponent,
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
    ],
})
export class FieldEditorComponent {
    private readonly subscriptions = new Subscriptions();
    private readonly isDisabledClear$ = new BehaviorSubject(true);
    private readonly isDisabledAI$ = new BehaviorSubject(true);
    private readonly isDisabledFullscreen$ = new BehaviorSubject(true);
    private readonly isVisibleAI$ = new BehaviorSubject(false);

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
    public formModel!: AbstractContentForm<AnyFieldDto, AbstractControl>;

    @Input({ required: true, transform: booleanAttribute })
    public menuShowCustom = true;

    @Input({ required: true })
    public menuItems: MenuItem[] = [];

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

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public isDisabled!: Observable<boolean>;
    public isEmpty!: Observable<boolean>;
    public isExpanded = false;
    public chatDialog = new DialogModel();
    public annotations?: Observable<ReadonlyArray<Annotation>>;

    public get field() {
        return this.formModel.field;
    }

    public get fieldForm() {
        return this.formModel.form;
    }

    public get schemaIds() {
        return Types.is(this.field.properties, ReferencesFieldPropertiesDto) ? this.field.properties.schemaIds : undefined;
    }

    public readonly defaultMenuItems: MenuItem[] = [
        {
            key: 'chat',
            isDisabled: this.isDisabledAI$,
            isVisible: this.isVisibleAI$,
            menuLabel: 'i18n:contents.fieldAIMenu',
            label: 'AI',
            onClick: () => this.chatDialog.show(),
            tabIndex: -1,
        },
        {
            key: 'fullscreen',
            icon: 'fullscreen',
            isDisabled: this.isDisabledFullscreen$,
            menuLabel: 'i18n:contents.fieldFullscreenMenu',
            onClick: () => this.toggleExpanded(),
            tabIndex: -1,
            tooltip: 'i18n:contents.fieldFullscreen',
        },
        {
            key: 'unset',
            confirmRememberKey: 'unsetValue',
            confirmText:'i18n:contents.unsetValueConfirmText',
            confirmTitle:'i18n:contents.unsetValueConfirmTitle',
            icon: 'close',
            isDisabled: this.isDisabledClear$,
            menuLabel: 'i18n:contents.unsetValue',
            onClick: () => this.unset(),
            tabIndex: -1,
            tooltip: 'i18n:contents.unsetValue',
        },
    ];

    public allMenuItems: MenuItem[] = this.defaultMenuItems;

    constructor(
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.isDisabled = disabled$(this.formModel.form);
            this.isEmpty = hasNoValue$(this.formModel.form);

            this.subscriptions.unsubscribeAll();
            this.subscriptions.add(this.isDisabled.subscribe(this.updateMenu));
            this.subscriptions.add(this.isEmpty.subscribe(this.updateMenu));
        }

        if (changes.formModel || changes.comments) {
            this.annotations = this.comments?.getAnnotations(this.formModel.path);
        }

        if (changes.menuItems) {
            this.allMenuItems = [this.defaultMenuItems[0], ...this.menuItems, ...this.defaultMenuItems.slice(1)];
        }

        this.updateMenu();
    }

    private updateMenu() {
        const isDisabled = this.formModel.form.disabled;

        this.isDisabledAI$.next(isDisabled || this.isCollapsed || !this.hasChatBot );
        this.isDisabledClear$.next(isDisabled || this.isCollapsed || isValidValue(this.formModel.form.value));
        this.isDisabledFullscreen$.next(isDisabled || this.isCollapsed);
        this.isVisibleAI$.next(this.field.properties.fieldType === 'String');
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

    public annotationCreate(annotation: AnnotationSelection) {
        this.messageBus.emit(new AnnotationCreate(this.formModel.path, annotation));
    }

    public annotationsSelect(annotation: ReadonlyArray<string>) {
        this.messageBus.emit(new AnnotationsSelect(annotation));
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
