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
    selector: 'sqx-field-form',
    styleUrls: ['./field-form.component.scss'],
    templateUrl: './field-form.component.html',
})
export class FieldFormComponent implements AfterViewInit {
    @Input()
    public showButtons?: boolean | null;

    @Input()
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

    @Input()
    public isLocalizable?: boolean | null;

    @Output()
    public close = new EventEmitter();

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
