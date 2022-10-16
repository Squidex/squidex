/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Host, Input, OnChanges, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective, UntypedFormArray } from '@angular/forms';
import { merge } from 'rxjs';
import { LocalizerService, StatefulComponent, Types } from '@app/framework/internal';
import { formatError } from './error-formatting';
import { touchedChange$ } from './forms-helper';

interface State {
    // The error messages to show.
    errorMessages: ReadonlyArray<string>;
}

@Component({
    selector: 'sqx-control-errors[for]',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ControlErrorsComponent extends StatefulComponent<State> implements OnChanges, OnDestroy {
    private controlDisplayName = '';
    private control: AbstractControl | null = null;

    @Input()
    public for!: string | AbstractControl;

    @Input()
    public fieldName: string | null | undefined;

    public get isTouched() {
        return this.control?.touched || Types.is(this.control, UntypedFormArray);
    }

    constructor(changeDetector: ChangeDetectorRef,
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective,
        private readonly localizer: LocalizerService,
    ) {
        super(changeDetector, {
            errorMessages: [],
        });
    }

    public ngOnChanges() {
        const previousControl = this.control;

        if (this.fieldName) {
            this.controlDisplayName = this.fieldName;
        } else if (this.for) {
            if (Types.isString(this.for)) {
                let translation = this.localizer.get(`common.${this.for}`)!;

                if (!translation) {
                    translation = this.for.substring(0, 1).toUpperCase() + this.for.substring(1);
                }

                this.controlDisplayName = translation;
            } else {
                this.controlDisplayName = this.localizer.get('common.field')!;
            }
        }

        if (Types.isString(this.for)) {
            if (this.formGroupDirective && this.formGroupDirective.form) {
                this.control = this.formGroupDirective.form.controls[this.for];
            } else {
                this.control = null;
            }
        } else {
            this.control = this.for;
        }

        if (this.control !== previousControl) {
            this.unsubscribeAll();

            if (this.control) {
                this.own(
                    merge(
                        this.control.valueChanges,
                        this.control.statusChanges,
                        touchedChange$(this.control),
                    ).subscribe(() => {
                        this.createMessages();
                    }));
            }
        }

        this.createMessages();
    }

    private createMessages() {
        const errorMessages: string[] = [];

        if (this.control && this.control.invalid && this.isTouched && this.control.errors) {
            for (const [key, error] of Object.entries(this.control.errors)) {
                const message = formatError(this.localizer, this.controlDisplayName, key, error, this.control.value);

                if (Types.isString(message)) {
                    errorMessages.push(message);
                } else if (Types.isArray(message)) {
                    for (const error of message) {
                        errorMessages.push(error);
                    }
                }
            }
        }

        if (errorMessages.length === 0 && this.snapshot.errorMessages.length === 0) {
            return;
        }

        this.next({ errorMessages });
    }
}
