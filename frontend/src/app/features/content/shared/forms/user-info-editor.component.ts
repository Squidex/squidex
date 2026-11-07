/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, model, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { disabled, form, FormField, FormValueControl, required } from '@angular/forms/signals';
import { CodeComponent, FormHintComponent, FormRowComponent, generateApiKey, MarkdownDirective, RolesState, SignalSelectDirective, TranslatePipe, Types } from '@app/shared';

type UserInfo = { apiKey: string; role: string };

@Component({
    selector: 'sqx-user-info-editor',
    styleUrls: ['./user-info-editor.component.scss'],
    templateUrl: './user-info-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class UserInfoEditorComponent implements FormValueControl<UserInfo | null> {
    private readonly userInfo = signal<UserInfo>({ apiKey: '', role: '' });

    protected readonly rolesState = inject(RolesState);

    /** The value of the control. */
    public readonly value = model<UserInfo | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The prefix for the IDs of the inner inputs. */
    public readonly formId = input.required<string>();

    protected readonly userInfoForm = form(this.userInfo, path => {
        disabled(path, () => this.disabled());
        required(path.apiKey);
        required(path.role);
    });

    protected readonly apiKey = computed(() => {
        return this.userInfo().apiKey || 'NONE';
    });

    constructor() {
        this.rolesState.loadIfNotLoaded();

        effect(() => {
            const value = this.value();

            untracked(() => {
                if (Types.equals(value, this.buildValue())) {
                    return;
                }

                if (Types.isObject(value)) {
                    this.userInfo.set({ apiKey: value.apiKey ?? '', role: value.role ?? '' });
                } else {
                    this.userInfo.set({ apiKey: '', role: '' });
                }
            });
        });

        effect(() => {
            const value = this.buildValue();

            untracked(() => {
                if (Types.equals(value, this.value())) {
                    return;
                }

                this.value.set(value);
            });
        });
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
