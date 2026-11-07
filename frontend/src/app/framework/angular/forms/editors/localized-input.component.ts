/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SignalControlComponent, Types } from '@app/framework/internal';
import { Language, LanguageSelectorComponent } from '../../language-selector.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IndeterminateValueDirective } from '../indeterminate-value.directive';
import { DateTimeEditorComponent } from './date-time-editor.component';
import { TagEditorComponent } from './tag-editor.component';

export const SQX_LOCALIZED_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => LocalizedInputComponent), multi: true,
};

const DEFAULT_LANGUAGE = { iso2Code: 'iv', englishName: 'Invariant' };

type LocalizedValue = { [key: string]: any };

@Component({
    selector: 'sqx-localized-input',
    styleUrls: ['./localized-input.component.scss'],
    templateUrl: './localized-input.component.html',
    providers: [
        SQX_LOCALIZED_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        FormsModule,
        IndeterminateValueDirective,
        LanguageSelectorComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class LocalizedInputComponent extends SignalControlComponent<LocalizedValue> {
    /** The languages to edit a value for. */
    public readonly languages = input.required<ReadonlyArray<Language>>();

    /** The editor to use for each value. */
    public readonly type = input<'text' | 'boolean' | 'datetime' | 'date' | 'tags' | 'number'>('text');

    /** The name attribute of the inner input. */
    public readonly name = input('');

    /** The ID of the inner input. */
    public readonly id = input('');

    public readonly language = signal<Language>(DEFAULT_LANGUAGE);

    public readonly value = signal<LocalizedValue | undefined>(undefined);

    public readonly valueCurrent = computed(() => {
        return this.value()?.[this.language().iso2Code];
    });

    public readonly valueIsEmpty = computed(() => {
        const value = this.value();
        if (!value) {
            return true;
        }

        return !value.hasOwnProperty(this.language().iso2Code);
    });

    public setLanguage(language: Language) {
        this.language.set(language);
    }

    public writeValue(obj: any) {
        this.value.set(Types.isObject(obj) ? obj : {});
    }

    public setValue(value: any) {
        const result = { ...this.value() || {}, [this.language().iso2Code]: value };

        this.value.set(result);
        this.callChange(result);
    }

    public unset() {
        let result: LocalizedValue | undefined = { ...this.value() || {} };

        delete result[this.language().iso2Code];

        if (Object.keys(result).length === 0) {
            result = undefined;
        }

        this.value.set(result);
        this.callChange(result);
    }
}