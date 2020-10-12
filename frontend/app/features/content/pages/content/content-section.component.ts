/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldForm, FieldSection, LocalStoreService, RootFieldDto, SchemaDto, Settings } from '@app/shared';

@Component({
    selector: 'sqx-content-section',
    styleUrls: ['./content-section.component.scss'],
    templateUrl: './content-section.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentSectionComponent implements OnChanges {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public compact = false;

    @Input()
    public form: EditContentForm;

    @Input()
    public formCompare?: EditContentForm;

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

    public isCollapsed: boolean;

    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnChanges() {
        this.isCollapsed = this.localStore.getBoolean(this.configKey());
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(this.configKey(), this.isCollapsed);
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