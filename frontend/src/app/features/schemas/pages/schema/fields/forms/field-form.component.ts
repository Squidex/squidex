/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, Output } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form[field][fieldForm][languages][schema][settings]',
    styleUrls: ['./field-form.component.scss'],
    templateUrl: './field-form.component.html',
})
export class FieldFormComponent implements AfterViewInit {
    @Input()
    public showButtons?: boolean | null;

    @Input()
    public isEditable?: boolean | null;

    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public settings!: AppSettingsDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    @Output()
    public cancel = new EventEmitter();

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
