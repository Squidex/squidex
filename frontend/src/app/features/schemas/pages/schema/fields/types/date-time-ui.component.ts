/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { DATETIME_FIELD_EDITORS, DateTimeFieldPropertiesDto, FieldDto, FloatConverter, FormHintComponent, MarkdownInlinePipe, SafeHtmlPipe, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html',
    imports: [
        FormHintComponent,
        FormsModule,
        MarkdownInlinePipe,
        ReactiveFormsModule,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class DateTimeUIComponent {
    public readonly converter = FloatConverter.INSTANCE;
    public readonly editors = DATETIME_FIELD_EDITORS;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: DateTimeFieldPropertiesDto;
}
