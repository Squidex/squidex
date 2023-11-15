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
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        FormHintComponent,
        AssetFolderDropdownComponent,
        MarkdownInlinePipe,
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
