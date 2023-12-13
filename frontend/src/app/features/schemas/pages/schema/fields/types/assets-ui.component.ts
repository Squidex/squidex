/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AssetFolderDropdownComponent, AssetsFieldPropertiesDto, FieldDto, FormHintComponent, MarkdownInlinePipe, SafeHtmlPipe, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html',
    imports: [
        AssetFolderDropdownComponent,
        FormHintComponent,
        FormsModule,
        MarkdownInlinePipe,
        ReactiveFormsModule,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class AssetsUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: AssetsFieldPropertiesDto;
}
