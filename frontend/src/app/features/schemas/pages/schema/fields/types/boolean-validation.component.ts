/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { BooleanFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html',
})
export class BooleanValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: BooleanFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public showDefaultValue?: Observable<boolean>;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.fieldForm) {
            this.showDefaultValue =
                hasNoValue$(this.fieldForm.controls['isRequired']);
        }
    }
}
