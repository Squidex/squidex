/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { fadeAnimation, FieldDto, hasNoValue$, hasValue$, LanguageDto, ModalModel, PatternDto, ResourceOwner, RootFieldDto, StringFieldPropertiesDto, STRING_CONTENT_TYPES, Types, value$ } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    animations: [
        fadeAnimation
    ]
})
export class StringValidationComponent extends ResourceOwner implements OnChanges, OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: StringFieldPropertiesDto;

    @Input()
    public patterns: ReadonlyArray<PatternDto>;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable: boolean;

    public contentTypes = STRING_CONTENT_TYPES;

    public showPatternMessage: Observable<boolean>;
    public showPatternSuggestions: Observable<boolean>;

    public patternName: string;
    public patternsModal = new ModalModel();

    public showUnique: boolean;

    public ngOnInit() {
        this.showUnique = Types.is(this.field, RootFieldDto) && !this.field.isLocalizable;

        if (this.showUnique) {
            this.fieldForm.setControl('isUnique',
                new FormControl(this.properties.isUnique));
        }

        this.fieldForm.setControl('maxLength',
            new FormControl(this.properties.maxLength));

        this.fieldForm.setControl('minLength',
            new FormControl(this.properties.minLength));

        this.fieldForm.setControl('maxWords',
            new FormControl(this.properties.maxWords));

        this.fieldForm.setControl('minWords',
            new FormControl(this.properties.minWords));

        this.fieldForm.setControl('maxCharacters',
            new FormControl(this.properties.maxCharacters));

        this.fieldForm.setControl('minCharacters',
            new FormControl(this.properties.minCharacters));

        this.fieldForm.setControl('contentType',
            new FormControl(this.properties.contentType));

        this.fieldForm.setControl('pattern',
            new FormControl(this.properties.pattern));

        this.fieldForm.setControl('patternMessage',
            new FormControl(this.properties.patternMessage));

        this.fieldForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.fieldForm.setControl('defaultValues',
            new FormControl(this.properties.defaultValues));

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

        this.setPatternName();
    }

    public ngOnChanges() {
        this.setPatternName();
    }

    public setPattern(pattern: PatternDto) {
        this.fieldForm.controls['pattern'].setValue(pattern.pattern);
        this.fieldForm.controls['patternMessage'].setValue(pattern.message);
    }

    private setPatternName() {
        const value = this.fieldForm.controls['pattern']?.value;

        if (!value) {
            this.patternName = '';
        } else {
            const matchingPattern = this.patterns.find(x => x.pattern === this.fieldForm.controls['pattern'].value);

            if (matchingPattern) {
                this.patternName = matchingPattern.name;
            } else {
                this.patternName = 'Advanced';
            }
        }
    }
}