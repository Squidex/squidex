/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, input, output } from '@angular/core';
import { AppLanguageDto, CursorsComponent, CursorsDirective, EditContentForm, FormErrorComponent, ListViewComponent, MarkdownDirective, SchemaDto, TranslatePipe, Version } from '@app/shared';
import { ContentSectionComponent } from '../../../shared/forms/content-section.component';

@Component({
    selector: 'sqx-content-editor',
    styleUrls: ['./content-editor.component.scss'],
    templateUrl: './content-editor.component.html',
    imports: [
        ContentSectionComponent,
        CursorsComponent,
        CursorsDirective,
        FormErrorComponent,
        ListViewComponent,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class ContentEditorComponent {
    /** Emits when the user selects another language. */
    public readonly languageChange = output<AppLanguageDto>();

    /** Emits when the user wants to view the latest version instead of an older version. */
    public readonly loadLatest = output();

    /** Emits when the user changes the ID of a new content. */
    public readonly contentIdChange = output<string>();

    /** Indicates that a new content is created. */
    public readonly isNew = input(false, { transform: booleanAttribute });

    /** Indicates that the content has been deleted. */
    public readonly isDeleted = input(false, { transform: booleanAttribute });

    /** Shows the input for a custom ID when a new content is created. */
    public readonly showIdInput = input(false, { transform: booleanAttribute });

    /** The ID of the content. */
    public readonly contentId = input.required<string>();

    /** The form of the content. */
    public readonly contentForm = input.required<EditContentForm>();

    /** The version of the content, when an older version is shown. */
    public readonly contentVersion = input<Version | null>();

    /** The form of the other version, when comparing two versions. */
    public readonly contentFormCompare = input<EditContentForm | null>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The selected language. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();
}
