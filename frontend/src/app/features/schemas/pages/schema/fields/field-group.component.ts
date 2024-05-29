/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppSettingsDto, FieldDto, FieldGroup, LanguageDto, LocalStoreService, RootFieldDto, SchemaDto, Settings, StatefulComponent } from '@app/shared';
import { FieldComponent } from './field.component';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-field-group',
    styleUrls: ['./field-group.component.scss'],
    templateUrl: './field-group.component.html',
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        FieldComponent,
    ],
})
export class FieldGroupComponent extends StatefulComponent<State> {
    @Output()
    public sorted = new EventEmitter<CdkDragDrop<FieldDto[]>>();

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public parent?: RootFieldDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input({ transform: booleanAttribute })
    public sortable = false;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ transform: booleanAttribute })
    public fieldsEmpty = false;

    @Input({ required: true })
    public fieldGroup!: FieldGroup;

    public trackByFieldFn: (_index: number, field: FieldDto) => any;

    public get hasAnyFields() {
        return this.parent ? this.parent.nested.length > 0 : this.schema.fields.length > 0;
    }

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
        super({ isCollapsed: false });

        this.changes.subscribe(change => {
            if (this.fieldGroup?.separator && this.schema) {
                this.localStore.setBoolean(this.isCollapsedKey(), change.snapshot.isCollapsed);
            }
        });

        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnInit() {
        if (this.fieldGroup?.separator && this.schema) {
            const isCollapsed = this.localStore.getBoolean(this.isCollapsedKey());

            this.next({ isCollapsed });
        }
    }

    public toggle() {
        this.next(s => ({
            ...s,
            isCollapsed: !s.isCollapsed,
        }));
    }

    public trackByField(_index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.schema?.id, this.fieldGroup.separator?.fieldId);
    }
}
