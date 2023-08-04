/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, ValidatorFn, Validators } from '@angular/forms';
import { Keys } from '@app/framework/internal';

@Component({
    selector: 'sqx-editable-title',
    styleUrls: ['./editable-title.component.scss'],
    templateUrl: './editable-title.component.html',
})
export class EditableTitleComponent {
    @Output()
    public inputTitleChange = new EventEmitter<string>();

    @Input({ required: true })
    public inputTitle!: string;

    @Input()
    public inputTitleLength = 20;

    @Input()
    public inputTitleRequired = true;

    @Input()
    public disabled?: boolean | null;

    @Input()
    public closeButton = true;

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'md';

    @Input()
    public displayFallback = '';

    public renaming = false;
    public renameForm = new FormControl<string>('');

    public onKeyDown(event: KeyboardEvent) {
        if (Keys.isEscape(event)) {
            this.toggleRename();
        }
    }

    public toggleRename() {
        if (this.disabled) {
            return;
        }

        if (!this.renaming) {
            let validators: ValidatorFn[] = [];

            if (this.inputTitleLength) {
                validators.push(Validators.maxLength(this.inputTitleLength));
            }

            if (this.inputTitleRequired) {
                validators.push(Validators.required);
            }

            this.renameForm.setValidators(validators);
        }

        this.renameForm.setValue(this.inputTitle || '');
        this.renaming = !this.renaming;
    }

    public rename() {
        if (this.disabled) {
            return;
        }

        if (this.renameForm.valid) {
            const text = this.renameForm.value || '';

            this.inputTitleChange.emit(text);
            this.inputTitle = text;

            this.renaming = false;
        }
    }
}
