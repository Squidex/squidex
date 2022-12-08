/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppSettingsDto, FieldDto, FieldGroup, LanguageDto, LocalStoreService, RootFieldDto, SchemaDto, Settings, StatefulComponent } from '@app/shared';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    selector: 'sqx-field-group[fieldGroup][languages][schema][settings]',
    styleUrls: ['./field-group.component.scss'],
    templateUrl: './field-group.component.html',
})
export class FieldGroupComponent extends StatefulComponent<State> {
    @Output()
    public sorted = new EventEmitter<CdkDragDrop<FieldDto[]>>();

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public parent?: RootFieldDto;

    @Input()
    public settings!: AppSettingsDto;

    @Input()
    public sortable = false;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public fieldsEmpty = false;

    @Input()
    public fieldGroup!: FieldGroup;

    public trackByFieldFn: (_index: number, field: FieldDto) => any;

    public get hasAnyFields() {
        return this.parent ? this.parent.nested.length > 0 : this.schema.fields.length > 0;
    }

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
    ) {
        super(changeDetector, {
            isCollapsed: false,
        });

        this.changes.subscribe(state => {
            if (this.fieldGroup?.separator && this.schema) {
                this.localStore.setBoolean(this.isCollapsedKey(), state.isCollapsed);
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