/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, NumberFieldPropertiesDto, RootFieldDto, SchemaDto, Types } from '@app/shared';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html',
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
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public get showUnique() {
        return Types.is(this.field, RootFieldDto) && !this.field.isLocalizable && this.schema.type !== 'Component';
    }
}
