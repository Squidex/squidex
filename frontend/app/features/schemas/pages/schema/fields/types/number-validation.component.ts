/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, NumberFieldPropertiesDto, RootFieldDto, SchemaDto, Types } from '@app/shared';

@Component({
    selector: 'sqx-number-validation[field][fieldForm][properties][schema]',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html',
})
export class NumberValidationComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public schema: SchemaDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public get showUnique() {
        return Types.is(this.field, RootFieldDto) && !this.field.isLocalizable && this.schema.type !== 'Component';
    }
}
