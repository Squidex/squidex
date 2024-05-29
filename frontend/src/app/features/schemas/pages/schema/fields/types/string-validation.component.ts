/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { AppSettingsDto, DropdownMenuComponent, FieldDto, FormHintComponent, hasNoValue$, hasValue$, LanguageDto, LocalizedInputComponent, ModalDirective, ModalModel, ModalPlacementDirective, PatternDto, RootFieldDto, SchemaDto, STRING_CONTENT_TYPES, StringFieldPropertiesDto, Subscriptions, TranslatePipe, TypedSimpleChanges, Types, value$ } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    imports: [
        AsyncPipe,
        DropdownMenuComponent,
        FormHintComponent,
        FormsModule,
        LocalizedInputComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class StringValidationComponent  {
    private readonly subscriptions = new Subscriptions();

    public readonly contentTypes = STRING_CONTENT_TYPES;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public properties!: StringFieldPropertiesDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;

    public showPatternMessage?: Observable<boolean>;
    public showPatternSuggestions?: Observable<boolean>;

    public patternName = '';
    public patternsModal = new ModalModel();

    public get showUnique() {
        return Types.is(this.field, RootFieldDto) && !this.field.isLocalizable && this.schema.type !== 'Component';
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.settings) {
            this.setPatternName();
        }

        if (changes.fieldForm) {
            this.subscriptions.unsubscribeAll();

            this.showPatternSuggestions =
                hasNoValue$(this.fieldForm.controls['pattern']);

            this.showPatternSuggestions =
                hasNoValue$(this.fieldForm.controls['pattern']);

            this.showPatternMessage =
                hasValue$(this.fieldForm.controls['pattern']);

            this.subscriptions.add(
                value$(this.fieldForm.controls['pattern'])
                    .subscribe((value: string) => {
                        if (!value) {
                            this.fieldForm.controls['patternMessage'].setValue(undefined);
                        }

                        this.setPatternName();
                    }));
        }
    }

    public setPattern(pattern: PatternDto) {
        this.fieldForm.controls['pattern'].setValue(pattern.regex);
        this.fieldForm.controls['patternMessage'].setValue(pattern.message);
    }

    private setPatternName() {
        const regex = this.fieldForm.controls['pattern']?.value;

        if (!regex) {
            this.patternName = '';
        } else {
            const matchingPattern = this.settings.patterns.find(x => x.regex === regex);

            if (matchingPattern) {
                this.patternName = matchingPattern.name;
            } else {
                this.patternName = 'Advanced';
            }
        }
    }
}
