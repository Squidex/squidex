/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, model, signal } from '@angular/core';
import { Types } from '@app/framework/internal';
import { Language, LanguageSelectorComponent } from '../../language-selector.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { CheckboxComponent } from '../checkbox.component';
import { FormValueControlBase } from '../form-value-control';
import { DateTimeEditorComponent } from './date-time-editor.component';
import { TagEditorComponent } from './tag-editor.component';

const DEFAULT_LANGUAGE = { iso2Code: 'iv', englishName: 'Invariant' };

type LocalizedValue = { [key: string]: any };

@Component({
    selector: 'sqx-localized-input',
    styleUrls: ['./localized-input.component.scss'],
    templateUrl: './localized-input.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        CheckboxComponent,
        LanguageSelectorComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class LocalizedInputComponent extends FormValueControlBase<LocalizedValue | null> {
    /** The values per language. */
    public readonly value = model<LocalizedValue | null>(null);

    /** The languages to edit a value for. */
    public readonly languages = input.required<ReadonlyArray<Language>>();

    /** The editor to use for each value. */
    public readonly type = input<'text' | 'boolean' | 'datetime' | 'date' | 'tags' | 'number'>('text');

    /** The name attribute of the inner input. */
    public readonly name = input('');

    /** The ID of the inner input. */
    public readonly id = input('');

    protected readonly language = signal<Language>(DEFAULT_LANGUAGE);

    protected readonly valueObject = computed(() => {
        const value = this.value();

        return Types.isObject(value) ? value as LocalizedValue : null;
    });

    protected readonly valueCurrent = computed(() => {
        return this.valueObject()?.[this.language().iso2Code] ?? null;
    });

    protected readonly valueIsEmpty = computed(() => {
        const value = this.valueObject();
        if (!value) {
            return true;
        }

        return !value.hasOwnProperty(this.language().iso2Code);
    });

    protected setLanguage(language: Language) {
        this.language.set(language);
    }

    protected setValue(value: any) {
        const result = { ...this.valueObject() || {}, [this.language().iso2Code]: value };

        this.value.set(result);
    }

    protected unset() {
        const result = { ...this.valueObject() || {} };

        delete result[this.language().iso2Code];

        if (Object.keys(result).length === 0) {
            this.value.set(null);
        } else {
            this.value.set(result);
        }
    }
}
