/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto, SchemaDto, TranslatePipe } from '@app/shared';
import { JsonMoreComponent } from '../types/json-more.component';
import { FieldFormCommonComponent } from './field-form-common.component';
import { FieldFormUIComponent } from './field-form-ui.component';
import { FieldFormValidationComponent } from './field-form-validation.component';

@Component({
    standalone: true,
    selector: 'sqx-field-form',
    styleUrls: ['./field-form.component.scss'],
    templateUrl: './field-form.component.html',
    imports: [
        FieldFormCommonComponent,
        FieldFormUIComponent,
        FieldFormValidationComponent,
        JsonMoreComponent,
        TranslatePipe,
    ],
})
export class FieldFormComponent implements AfterViewInit {
    @Input({ transform: booleanAttribute })
    public showButtons?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isEditable?: boolean | null;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;

    @Output()
    public dialogClose = new EventEmitter();

    public selectedTab = 0;

    public ngAfterViewInit() {
        if (!this.isEditable) {
            this.fieldForm.disable();
        } else {
            this.fieldForm.enable();
        }
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }
}
