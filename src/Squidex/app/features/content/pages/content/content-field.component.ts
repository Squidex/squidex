/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, DoCheck, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

import {
    AppLanguageDto,
    EditContentForm,
    fieldInvariant,
    ImmutableArray,
    LocalStoreService,
    RootFieldDto,
    SchemaDto,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-content-field',
    styleUrls: ['./content-field.component.scss'],
    templateUrl: './content-field.component.html'
})
export class ContentFieldComponent implements DoCheck, OnChanges {
    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public schema: SchemaDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    @Output()
    public languageChange = new EventEmitter<AppLanguageDto>();

    public selectedFormControl: AbstractControl;
    public showAllControls = false;

    public isInvalid: Observable<boolean>;

    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.isInvalid = this.fieldForm.statusChanges.pipe(startWith(this.fieldForm.invalid), map(x => this.fieldForm.invalid));
        }

        if (changes['field']) {
            this.showAllControls = this.localStore.getBoolean(this.configKey());
        }
    }

    public toggleShowAll() {
        this.showAllControls = !this.showAllControls;

        this.localStore.setBoolean(this.configKey(), this.showAllControls);
    }

    public ngDoCheck() {
        let control: AbstractControl;

        if (this.field.isLocalizable) {
            control = this.fieldForm.controls[this.language.iso2Code];
        } else {
            control = this.fieldForm.controls[fieldInvariant];
        }

        if (this.selectedFormControl !== control) {
            if (this.selectedFormControl && Types.isFunction(this.selectedFormControl['_clearChangeFns'])) {
                this.selectedFormControl['_clearChangeFns']();
            }

            this.selectedFormControl = control;
        }
    }

    private configKey() {
        return `squidex.schemas.${this.schema.id}.fields.${this.field.fieldId}.show-all`;
    }
}

