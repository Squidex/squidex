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
    @Output()
    public nameChange = new EventEmitter<string>();

    @Input()
    public disabled = false;

    @Input()
    public fallback: string;

    @Input()
    public name: string;

    @Input()
    public maxLength = 20;

    @Input()
    public set isRequired(value: boolean) {
        const validator =
            value ?
            Validators.required :
            Validators.nullValidator;

        this.renameForm.controls['name'].setValidators(validator);
    }

    public renaming = false;
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

        this.renameForm.setValue({ name: this.name || '' });
        this.renaming = !this.renaming;
    }

    public rename() {
        if (this.disabled) {
            return;
        }

        if (this.renameForm.valid) {
            const value = this.renameForm.value;

            this.nameChange.emit(value.name);
            this.name = value.name;

            this.renaming = false;
        }
    }
}