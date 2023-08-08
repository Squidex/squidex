/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
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

    @Output()
    public contentIdChange = new EventEmitter<string>();

    @Input({ transform: booleanAttribute })
    public isNew = false;

    @Input({ transform: booleanAttribute })
    public isDeleted?: boolean;

    @Input({ transform: booleanAttribute })
    public showIdInput = false;

    @Input({ required: true })
    public contentId!: string;

    @Input({ required: true })
    public contentForm!: EditContentForm;

    @Input()
    public contentVersion?: Version | null;

    @Input()
    public contentFormCompare?: EditContentForm | null;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    public trackBySection(_index: number, section: FieldSection<RootFieldDto, FieldForm>) {
        return section.separator?.fieldId;
    }
}
