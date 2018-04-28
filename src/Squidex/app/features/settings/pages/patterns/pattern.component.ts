/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppPatternDto,
    EditPatternForm,
    PatternsState
} from '@app/shared';

@Component({
    selector: 'sqx-pattern',
    styleUrls: ['./pattern.component.scss'],
    templateUrl: './pattern.component.html'
})
export class PatternComponent implements OnInit {
    @Input()
    public pattern: AppPatternDto;

    public editForm = new EditPatternForm(this.formBuilder);

    constructor(
        private readonly patternsState: PatternsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.editForm.load(this.pattern);
    }

    public cancel() {
        this.editForm.submitCompleted(this.pattern);
    }

    public delete() {
        this.patternsState.delete(this.pattern).onErrorResumeNext().subscribe();
    }

    public save() {
        const value = this.editForm.submit();

        if (value) {
            if (this.pattern) {
                this.patternsState.update(this.pattern, value)
                    .subscribe(() => {
                        this.editForm.submitCompleted();
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            } else {
                this.patternsState.create(value)
                    .subscribe(() => {
                        this.editForm.submitCompleted({});
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            }
        }
    }
}

