/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { AppLanguageDto, BooleanFieldPropertiesDto, FieldDto, FormRowComponent, hasNoValue$, IndeterminateValueDirective, LocalizedInputComponent, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        IndeterminateValueDirective,
        LocalizedInputComponent,
        ReactiveFormsModule,
    ],
})
export class BooleanValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: BooleanFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ required: true, transform: booleanAttribute })
    public isLocalizable!: boolean;

    public showDefaultValue?: Observable<boolean>;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.fieldForm) {
            this.showDefaultValue =
                hasNoValue$(this.fieldForm.controls['isRequired']);
        }
    }
}
