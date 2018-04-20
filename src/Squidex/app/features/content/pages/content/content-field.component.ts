/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';

import {
    AppLanguageDto,
    FieldDto,
    fieldInvariant,
    ImmutableArray
} from '@app/shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentFieldComponent implements OnChanges {
    @Input()
    public field: FieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Input()
    public contentFormSubmitted: boolean;

    public selectedFormControl: AbstractControl;

    public ngOnChanges() {
        if (this.field.isLocalizable) {
            this.selectedFormControl = this.fieldForm.controls[this.language.iso2Code];
            this.selectedFormControl['_clearChangeFns']();
        } else {
            this.selectedFormControl = this.fieldForm.controls[fieldInvariant];
        }
    }
}

