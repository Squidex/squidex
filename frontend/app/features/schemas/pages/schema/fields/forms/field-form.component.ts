/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, LanguageDto, PatternDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form',
    styleUrls: ['./field-form.component.scss'],
    templateUrl: './field-form.component.html'
})
export class FieldFormComponent implements AfterViewInit {
    @Input()
    public showButtons: boolean;

    @Input()
    public isEditable: boolean;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public patterns: ReadonlyArray<PatternDto>;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable: boolean;

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