/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { fadeAnimation, FieldDto, hasNoValue$, hasValue$, ModalModel, PatternDto, ResourceOwner, RootFieldDto, StringFieldPropertiesDto, Types, value$ } from '@app/shared';
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

    public showDefaultValue: Observable<boolean>;
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

        this.fieldForm.setControl('pattern',
            new FormControl(this.properties.pattern));

        this.fieldForm.setControl('patternMessage',
            new FormControl(this.properties.patternMessage));

        this.fieldForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.showDefaultValue =
            hasNoValue$(this.fieldForm.controls['isRequired']);

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