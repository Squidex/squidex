/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, input, untracked } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { switchMap } from 'rxjs/operators';
import { AppLanguageDto, AppSettingsDto, DropdownMenuComponent, FieldDto, FormRowComponent, hasNoValue$, hasValue$, LocalizedInputComponent, ModalDirective, ModalModel, ModalPlacementDirective, PatternDto, SchemaDto, StringContentTypeValues, StringFieldPropertiesDto, TranslatePipe, Types, value$ } from '@app/shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class StringValidationComponent {
    private readonly patternControl = computed(() => {
        return this.fieldForm().controls['pattern'];
    });

    private readonly patternValue = toSignal(
        toObservable(this.patternControl).pipe(
            switchMap(control => value$<string>(control))));

    protected readonly contentTypes = StringContentTypeValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<StringFieldPropertiesDto>();

    /** The settings of the app, which provide the predefined patterns. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly patternsModal = new ModalModel();

    protected readonly showPatternMessage = toSignal(
        toObservable(this.patternControl).pipe(
            switchMap(control => hasValue$(control))),
        { initialValue: false });

    protected readonly showPatternSuggestions = toSignal(
        toObservable(this.patternControl).pipe(
            switchMap(control => hasNoValue$(control))),
        { initialValue: false });

    protected readonly patternName = computed(() => {
        const regex = this.patternValue();
        if (!regex) {
            return '';
        }

        const matchingPattern = this.settings().patterns.find(x => x.regex === regex);
        if (matchingPattern) {
            return matchingPattern.name;
        }

        return 'Advanced';
    });

    protected readonly showUnique = computed(() => {
        const field = this.field();

        return Types.is(field, FieldDto) && !field.isLocalizable && this.schema().type !== 'Component';
    });

    constructor() {
        effect(onCleanup => {
            const fieldForm = this.fieldForm();

            const subscription = untracked(() =>
                value$<string>(fieldForm.controls['pattern'])
                    .subscribe(value => {
                        if (!value) {
                            fieldForm.controls['patternMessage'].setValue(undefined);
                        }
                    }));

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected setPattern(pattern: PatternDto) {
        const fieldForm = this.fieldForm();

        fieldForm.controls['pattern'].setValue(pattern.regex);
        fieldForm.controls['patternMessage'].setValue(pattern.message);
    }
}
