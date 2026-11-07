/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormValueControl } from '@angular/forms/signals';
import { Types } from '@app/framework/internal';
import { Language, LanguageSelectorComponent } from '../../language-selector.component';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IndeterminateValueDirective } from '../indeterminate-value.directive';
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
        FormsModule,
        IndeterminateValueDirective,
        LanguageSelectorComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class LocalizedInputComponent implements FormValueControl<LocalizedValue | null> {
    private readonly valueObject = computed(() => {
        const value = this.value();

        return Types.isObject(value) ? value as LocalizedValue : null;
    });

    /** The values per language. */
    public readonly value = model<LocalizedValue | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The languages to edit a value for. */
    public readonly languages = input.required<ReadonlyArray<Language>>();

    /** The editor to use for each value. */
    public readonly type = input<'text' | 'boolean' | 'datetime' | 'date' | 'tags' | 'number'>('text');

    /** The name attribute of the inner input. */
    public readonly name = input('');

    /** The ID of the inner input. */
    public readonly id = input('');

    protected readonly language = signal<Language>(DEFAULT_LANGUAGE);

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
