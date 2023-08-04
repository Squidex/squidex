/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldForm, FieldSection, LocalStoreService, RootFieldDto, SchemaDto, Settings, StatefulComponent, TypedSimpleChanges } from '@app/shared';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    selector: 'sqx-content-section',
    styleUrls: ['./content-section.component.scss'],
    templateUrl: './content-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentSectionComponent extends StatefulComponent<State> {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public isCompact?: boolean | null;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

    @Input({ required: true })
    public formLevel!: number;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true })
    public formSection!: FieldSection<RootFieldDto, FieldForm>;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
    ) {
        super(changeDetector, {
            isCollapsed: false,
        });

        this.changes.subscribe(state => {
            if (this.formSection?.separator && this.schema) {
                this.localStore.setBoolean(this.isCollapsedKey(), state.isCollapsed);
            }
        });
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formSection || changes.schema) {
            if (this.formSection?.separator && this.schema) {
                const isCollapsed = this.localStore.getBoolean(this.isCollapsedKey());

                this.next({ isCollapsed });
            }
        }
    }

    public toggle() {
        this.next(s => ({
            ...s,
            isCollapsed: !s.isCollapsed,
        }));
    }

    public getFieldFormCompare(formState: FieldForm) {
        return this.formCompare?.get(formState.field.name);
    }

    public trackByField(_index: number, formState: FieldForm) {
        return formState.field.fieldId;
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.schema?.id, this.formSection?.separator?.fieldId);
    }
}
