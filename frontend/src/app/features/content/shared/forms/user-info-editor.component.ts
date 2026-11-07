/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, forwardRef, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl, Validators } from '@angular/forms';
import { map } from 'rxjs';
import { CodeComponent, ExtendedFormGroup, FormHintComponent, FormRowComponent, generateApiKey, MarkdownDirective, RolesState, SignalControlComponent, TranslatePipe, Types, value$ } from '@app/shared';

export const SQX_USER_INFO_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => UserInfoEditorComponent), multi: true,
};

type UserInfo = { apiKey: string; role: string };

@Component({
    selector: 'sqx-user-info-editor',
    styleUrls: ['./user-info-editor.component.scss'],
    templateUrl: './user-info-editor.component.html',
    providers: [
        SQX_USER_INFO_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeComponent,
        FormsModule,
        FormHintComponent,
        MarkdownDirective,
        ReactiveFormsModule,
        FormRowComponent,
        TranslatePipe,
    ],
})
export class UserInfoEditorComponent extends SignalControlComponent<UserInfo> {
    /** The prefix for the IDs of the inner inputs. */
    public readonly formId = input.required<string>();

    protected readonly form = new ExtendedFormGroup({
        apiKey: new UntypedFormControl('',
            Validators.required,
        ),
        role: new UntypedFormControl('',
            Validators.required,
        ),
    });

    protected readonly apiKey = toSignal(
        value$(this.form.get('apiKey')!)
            .pipe(map(x => x || 'NONE')),
        { initialValue: 'NONE' });

    constructor(
        protected readonly rolesState: RolesState,
    ) {
        super();

        this.rolesState.loadIfNotLoaded();

        effect(onCleanup => {
            const subscription =
                this.form.valueChanges.subscribe(value => {
                    if (this.form.valid) {
                        this.callChange(value);
                    } else {
                        this.callChange(null);
                    }

                    this.callTouched();
                });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public writeValue(obj: UserInfo | undefined | null) {
        if (Types.isObject(obj)) {
            this.form.setValue(obj, { emitEvent: true });
        } else {
            this.form.reset();
        }
    }

    protected async generateApiKey() {
        const apiKey = generateApiKey();

        this.form.patchValue({ apiKey });
    }
}