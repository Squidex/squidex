/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, LocalStoreService, RootFieldDto, SchemaDto } from '@app/shared';
import { FieldSection } from './../../shared/group-fields.pipe';

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
    public form: EditContentForm;

    @Input()
    public formCompare?: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public schema: SchemaDto;

    @Input()
    public section: FieldSection<RootFieldDto>;

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

    public changeCollapsed(value: boolean) {
        this.isCollapsed = value;

        this.localStore.setBoolean(this.configKey(), value);
    }

    public getFieldForm(field: RootFieldDto) {
        return this.form.form.get(field.name)!;
    }

    public getFieldFormCompare(field: RootFieldDto) {
        return this.formCompare?.form.get(field.name)!;
    }

    public trackByField(index: number, field: RootFieldDto) {
        return field.fieldId;
    }

    private configKey(): string {
        return `squidex.schemas.${this.schema?.id}.fields.${this.section?.separator?.fieldId}.closed`;
    }
}