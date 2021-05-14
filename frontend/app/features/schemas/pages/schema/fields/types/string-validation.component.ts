/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppSettingsDto, fadeAnimation, FieldDto, hasNoValue$, hasValue$, LanguageDto, ModalModel, PatternDto, ResourceOwner, RootFieldDto, StringFieldPropertiesDto, STRING_CONTENT_TYPES, Types, value$ } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    animations: [
        fadeAnimation,
    ],
})
export class StringValidationComponent extends ResourceOwner implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: StringFieldPropertiesDto;

    @Input()
    public settings: AppSettingsDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public contentTypes = STRING_CONTENT_TYPES;

    public showPatternMessage: Observable<boolean>;
    public showPatternSuggestions: Observable<boolean>;

    public patternName: string;
    public patternsModal = new ModalModel();

    public get showUnique() {
        return Types.is(this.field, RootFieldDto) && !this.field.isLocalizable;
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['settings']) {
            this.setPatternName();
        }

        if (changes['fieldForm']) {
            this.unsubscribeAll();

            this.showPatternSuggestions =
                hasNoValue$(this.fieldForm.controls['pattern']);

            this.showPatternSuggestions =
                hasNoValue$(this.fieldForm.controls['pattern']);

            this.showPatternMessage =
                hasValue$(this.fieldForm.controls['pattern']);

            this.own(
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
