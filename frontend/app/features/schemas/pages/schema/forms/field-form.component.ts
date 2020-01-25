/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto, PatternDto } from '@app/shared';

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
    public editForm: FormGroup;

    @Input()
    public editFormSubmitted: boolean;

    @Input()
    public patterns: ReadonlyArray<PatternDto>;

    @Input()
    public field: FieldDto;

    @Output()
    public cancel = new EventEmitter();

    public selectedTab = 0;

    public ngAfterViewInit() {
        if (!this.isEditable) {
            this.editForm.disable();
        } else {
            this.editForm.enable();
        }
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }
}