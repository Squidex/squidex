/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, numberAttribute, viewChild, viewChildren } from '@angular/core';
import { VirtualScrollerComponent, VirtualScrollerModule } from '@iharbeck/ngx-virtual-scroller';
import { combineLatest } from 'rxjs';
import { AppLanguageDto, ComponentsFieldPropertiesDto, ConfirmClickDirective, disabled$, DropdownMenuComponent, EditContentForm, FieldArrayForm, FormHintComponent, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, ObjectFormBase, SchemaDto, Settings, sorted, TooltipDirective, TranslatePipe, Types } from '@app/shared';
import { ArrayItemComponent } from './array-item.component';

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ArrayItemComponent,
        AsyncPipe,
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        ConfirmClickDirective,
        DropdownMenuComponent,
        FormHintComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
        VirtualScrollerModule,
    ],
})
export class ArrayEditorComponent {
    private readonly children = viewChildren(ArrayItemComponent);
    private readonly scroller = viewChild(VirtualScrollerComponent);

    /** Enables the AI features of the editors. */
    public readonly hasChatBot = input.required<boolean>();

    /** The content form the field belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the field, 0 for root fields. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The form model of the array or components field. */
    public readonly formModel = input.required<FieldArrayForm>();

    /** Renders the editor next to the editor of another version. */
    public readonly isComparing = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** Renders the items in fullscreen mode. */
    public readonly isExpanded = input(false, { transform: booleanAttribute });

    /** The language of the field value. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    public readonly schemasDropdown = new ModalModel();

    public readonly isArray = computed(() => {
        return !Types.is(this.formModel().field.properties, ComponentsFieldPropertiesDto);
    });

    public readonly schemasList = computed<ReadonlyArray<SchemaDto>>(() => {
        const formModel = this.formModel();

        const properties = formModel.field.properties;
        if (!Types.is(properties, ComponentsFieldPropertiesDto)) {
            return [];
        }

        return properties.schemaIds?.map(x => formModel.globals.schemas[x]).defined().sortedByString(x => x.displayName) || [];
    });

    public readonly isDisabled = computed(() => {
        return disabled$(this.formModel().form);
    });

    public readonly isDisabledOrFull = computed(() => {
        const formModel = this.formModel();
        const maxItems = formModel.field.rawProperties['maxItems'] || Number.MAX_VALUE;

        return combineLatest([
            this.isDisabled(),
            formModel.itemChanges,
        ], (disabled, items) => {
            return disabled || items.length >= maxItems;
        });
    });

    public readonly isCollapsedInitial = computed(() => {
        return this.localStore.getBoolean(this.isCollapsedKey()) || this.formLevel() > 0;
    });

    public readonly hasField = computed(() => {
        return (this.formModel().field as any)['nested']?.length > 0;
    });

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public removeItem(index: number) {
        this.formModel().removeItemAt(index);
    }

    public addCopy(value: ObjectFormBase) {
        this.formModel().addCopy(value);
    }

    public addItem() {
        this.formModel().addItem();
    }

    public addComponent(schema: SchemaDto) {
        this.formModel().addComponent(schema.id);
    }

    public clear() {
        this.formModel().setValue([]);
    }

    public sort(event: CdkDragDrop<ReadonlyArray<ObjectFormBase>>) {
        this.formModel().sort(sorted(event));

        this.reset();
    }

    public move(item: ObjectFormBase, index: number) {
        this.formModel().move(index, item);

        this.reset();
    }

    public collapseAll() {
        for (const item of this.formModel().items) {
            item.collapse();
        }

        if (this.formLevel() === 0) {
            this.localStore.setBoolean(this.isCollapsedKey(), true);
        }

        this.scroller()?.invalidateAllCachedMeasurements();
    }

    public expandAll() {
        for (const item of this.formModel().items) {
            item.expand();
        }

        if (this.formLevel() === 0) {
            this.localStore.setBoolean(this.isCollapsedKey(), false);
        }

        this.scroller()?.invalidateAllCachedMeasurements();
    }

    private reset() {
        this.children().forEach(child => {
            child.reset();
        });
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.form().schema?.id, this.formModel().field?.fieldId);
    }
}
