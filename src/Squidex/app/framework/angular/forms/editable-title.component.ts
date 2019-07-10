/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

const ESCAPE_KEY = 27;

@Component({
    selector: 'sqx-editable-title',
    styleUrls: ['./editable-title.component.scss'],
    templateUrl: './editable-title.component.html'
})
export class EditableTitleComponent {
    @Input()
    public disabled = false;

    @Input()
    public name: string;

    @Output()
    public nameChanged = new EventEmitter<string>();

    public isRenaming = false;

    public renameForm = this.formBuilder.group({
        name: ['',
            [
                Validators.required
            ]
        ]
    });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public onKeyDown(keyCode: number) {
        if (keyCode === ESCAPE_KEY) {
            this.toggleRename();
        }
    }

    public toggleRename() {
        if (this.disabled) {
            return;
        }

        this.renameForm.setValue({ name: this.name });

        this.isRenaming = !this.isRenaming;
    }

    public rename() {
        if (this.disabled) {
            return;
        }

        if (this.renameForm.valid) {
            const value = this.renameForm.value;

            this.nameChanged.emit(value.name);

            this.toggleRename();
        }
    }
}