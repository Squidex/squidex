/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input, model, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import { CodeComponent, FormHintComponent, FormRowComponent, FormValueControlBase, generateApiKey, MarkdownDirective, RolesState, SignalSelectDirective, syncExternalValue, TranslatePipe, Types } from '@app/shared';

type UserInfo = { apiKey: string; role: string };

@Component({
    selector: 'sqx-user-info-editor',
    styleUrls: ['./user-info-editor.component.scss'],
    templateUrl: './user-info-editor.component.html',
    imports: [
        CodeComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class UserInfoEditorComponent extends FormValueControlBase<UserInfo | null> {
    private readonly valueSetter = syncExternalValue(() => this.value, value => this.applyValue(value));

    protected readonly rolesState = inject(RolesState);

    /** The value of the control. */
    public readonly value = model<UserInfo | null>(null);

    /** The prefix for the IDs of the inner inputs. */
    public readonly formId = input.required<string>();

    protected readonly userInfo = signal<UserInfo>({ apiKey: '', role: '' });

    protected readonly apiKey = computed(() => {
        return this.userInfo().apiKey || 'NONE';
    });

    protected readonly userInfoForm = form(this.userInfo, path => {
        disabled(path,
            {when: () => this.disabled() }
        );

        required(path.apiKey);
        required(path.role);
    });

    constructor() {
        super();

        this.rolesState.loadIfNotLoaded();

        effect(() => {
            const value = this.buildValue();

            untracked(() => {
                if (Types.equals(value, this.value())) {
                    return;
                }

                this.valueSetter(value);
            });
        });
    }

    private applyValue(value: UserInfo | null) {
        if (Types.isObject(value)) {
            this.userInfo.set({ apiKey: value.apiKey ?? '', role: value.role ?? '' });
        } else {
            this.userInfo.set({ apiKey: '', role: '' });
        }
    }

    protected generateApiKey() {
        const apiKey = generateApiKey();

        this.userInfo.update(x => ({ ...x, apiKey }));
    }

    private buildValue(): UserInfo | null {
        if (!this.userInfoForm().valid()) {
            return null;
        }

        return { ...this.userInfo() };
    }
}
