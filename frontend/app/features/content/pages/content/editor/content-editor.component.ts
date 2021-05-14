/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldForm, FieldSection, RootFieldDto, SchemaDto, Version } from '@app/shared';

@Component({
    selector: 'sqx-content-editor',
    styleUrls: ['./content-editor.component.scss'],
    templateUrl: './content-editor.component.html',
})
export class ContentEditorComponent {
    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Output()
    public loadLatest = new EventEmitter<any>();

    @Input()
    public contentForm: EditContentForm;

    @Input()
    public contentVersion: Version | null;

    @Input()
    public contentFormCompare?: EditContentForm | null;

    @Input()
    public schema: SchemaDto;

    @Input()
    public formContext: any;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public language: AppLanguageDto;

    public trackBySection(_index: number, section: FieldSection<RootFieldDto, FieldForm>) {
        return section.separator?.fieldId;
    }
}
