/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { EditPatternForm, PatternDto, PatternsState } from '@app/shared';

@Component({
    selector: 'sqx-pattern',
    styleUrls: ['./pattern.component.scss'],
    templateUrl: './pattern.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PatternComponent implements OnChanges {
    @Input()
    public pattern: PatternDto;

    public editForm = new EditPatternForm(this.formBuilder);

    public isEditable = true;
    public isDeletable = false;

    constructor(
        private readonly patternsState: PatternsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['pattern']) {
            this.isEditable = !this.pattern || this.pattern.canUpdate;
            this.isDeletable = this.pattern && this.pattern.canDelete;

            this.editForm.load(this.pattern);
            this.editForm.setEnabled(this.isEditable);
        }
    }

    public cancel() {
        this.editForm.submitCompleted({ newValue: this.pattern });
    }

    public delete() {
        this.patternsState.delete(this.pattern);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            if (this.pattern) {
                this.patternsState.update(this.pattern, value)
                    .subscribe(() => {
                        this.editForm.submitCompleted({ noReset: true });
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            } else {
                this.patternsState.create(value)
                    .subscribe(() => {
                        this.editForm.submitCompleted();
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            }
        }
    }
}