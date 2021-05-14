/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldForm, FieldSection, LocalStoreService, RootFieldDto, SchemaDto, Settings, StatefulComponent } from '@app/shared';

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
export class ContentSectionComponent extends StatefulComponent<State> implements OnChanges {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public isCompact?: boolean | null;

    @Input()
    public form: EditContentForm;

    @Input()
    public formCompare?: EditContentForm | null;

    @Input()
    public formContext: any;

    @Input()
    public formSection: FieldSection<RootFieldDto, FieldForm>;

    @Input()
    public schema: SchemaDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
    ) {
        super(changeDetector, {
            isCollapsed: false,
        });

        this.changes.subscribe(state => {
            this.localStore.setBoolean(this.configKey(), state.isCollapsed);
        });
    }

    public ngOnChanges() {
        const isCollapsed = this.localStore.getBoolean(this.configKey());

        this.next({ isCollapsed });
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

    private configKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.schema?.id, this.formSection?.separator?.fieldId);
    }
}
