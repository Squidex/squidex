/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppPatternDto,
    fadeAnimation,
    ValidatorsEx,
    UpdatePatternDto
} from 'shared';

@Component({
    selector: 'sqx-pattern',
    styleUrls: ['./pattern.component.scss'],
    templateUrl: './pattern.component.html',
    animations: [
        fadeAnimation
    ]
})
export class PatternComponent implements OnInit {
    @Input()
    public isNew = false;

    @Input()
    public pattern: AppPatternDto;

    @Output()
    public removing = new EventEmitter<any>();

    @Output()
    public updating = new EventEmitter<UpdatePatternDto>();

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            name: [
                '',
                [
                    Validators.required,
                    Validators.maxLength(100),
                    ValidatorsEx.pattern('[A-z0-9]+[A-z0-9\- ]*[A-z0-9]', 'Name can only contain letters, numbers, dashes and spaces.')
                ]
            ],
            pattern: [
                '',
                [
                    Validators.required
                ]
            ],
            message: [
                '',
                [
                    Validators.maxLength(1000)
                ]
            ]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        const pattern = this.pattern;

        if (pattern) {
            this.editForm.setValue({ name: pattern.name, pattern: pattern.pattern, message: pattern.message || '' });
        }
    }

    public cancel() {
        this.editFormSubmitted = false;
        this.editForm.reset();
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const requestDto = new UpdatePatternDto(
                this.editForm.controls['name'].value,
                this.editForm.controls['pattern'].value,
                this.editForm.controls['message'].value);

            this.updating.emit(requestDto);

            if (!this.pattern) {
                this.cancel();
            }
        }
    }
}

