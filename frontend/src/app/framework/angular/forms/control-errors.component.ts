/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { ChangeDetectionStrategy, Component, Host, Input, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective, UntypedFormArray } from '@angular/forms';
import { merge } from 'rxjs';
import { LocalizerService, StatefulComponent, Subscriptions, Types } from '@app/framework/internal';
import { ControlErrorsMessagesComponent } from './control-errors-messages.component';
import { formatError } from './error-formatting';
import { touchedChange$ } from './forms-helper';

interface State {
    // The error messages to show.
    errorMessages: ReadonlyArray<string>;
}

@Component({
    standalone: true,
    selector: 'sqx-control-errors',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsMessagesComponent,
    ],
})
export class ControlErrorsComponent extends StatefulComponent<State> implements  OnDestroy {
    private readonly subscriptions = new Subscriptions();
    private controlDisplayName = '';
    private control: AbstractControl | null = null;

    @Input({ required: true })
    public for!: string | AbstractControl;

    @Input()
    public fieldName: string | null | undefined;

    public get isTouched() {
        return this.control?.touched || Types.is(this.control, UntypedFormArray);
    }

    constructor(
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective,
        private readonly localizer: LocalizerService,
    ) {
        super({ errorMessages: [] });
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
            this.subscriptions.unsubscribeAll();

            if (this.control) {
                this.subscriptions.add(
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
