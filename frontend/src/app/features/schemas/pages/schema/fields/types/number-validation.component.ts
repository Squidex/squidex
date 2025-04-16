/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, FieldDto, FormHintComponent, LocalizedInputComponent, NumberFieldPropertiesDto, SchemaDto, TranslatePipe, Types } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html',
    imports: [
        FormHintComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class NumberValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public properties!: NumberFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;

    public get showUnique() {
        return Types.is(this.field, FieldDto) && !this.field.isLocalizable && this.schema.type !== 'Component';
    }
}
