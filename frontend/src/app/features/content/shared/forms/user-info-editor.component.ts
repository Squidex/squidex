/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, FormRowComponent, generateApiKey, RolesState, StatefulControlComponent, Subscriptions, TranslatePipe, Types, value$ } from '@app/shared';

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
        AsyncPipe,
        FormsModule,
        ReactiveFormsModule,
        FormRowComponent,
        TranslatePipe,
    ],
})
export class UserInfoEditorComponent extends StatefulControlComponent<any, UserInfo | undefined | null> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    @Input({ required: true })
    public formId!: string;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public form = new ExtendedFormGroup({
        apiKey: new UntypedFormControl('',
            Validators.required,
        ),
        role: new UntypedFormControl('',
            Validators.required,
        ),
    });

    constructor(
        public readonly rolesState: RolesState,
    ) {
        super({});

        this.subscriptions.add(
            value$(this.form).subscribe(value => {
                if (this.form.valid) {
                    this.callChange(value);
                } else {
                    this.callChange(undefined);
                }

                this.callTouched();
            }));
    }

    public ngOnInit() {
        this.rolesState.loadIfNotLoaded();
    }

    public writeValue(obj: UserInfo | undefined | null) {
        if (Types.isObject(obj)) {
            this.form.setValue(obj);
        } else {
            this.form.reset();
        }
    }

    public async generateApiKey() {
        const apiKey = generateApiKey();

        this.form.patchValue({ apiKey });
    }
}