/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppLanguageDto, CursorsComponent, CursorsDirective, EditContentForm, FormErrorComponent, ListViewComponent, MarkdownInlinePipe, SafeHtmlPipe, SchemaDto, TranslatePipe, Version } from '@app/shared';
import { ContentSectionComponent } from '../../../shared/forms/content-section.component';

@Component({
    standalone: true,
    selector: 'sqx-content-editor',
    styleUrls: ['./content-editor.component.scss'],
    templateUrl: './content-editor.component.html',
    imports: [
        AsyncPipe,
        ContentSectionComponent,
        CursorsComponent,
        CursorsDirective,
        FormErrorComponent,
        FormsModule,
        ListViewComponent,
        MarkdownInlinePipe,
        SafeHtmlPipe,
        TranslatePipe,
    ],
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
}
