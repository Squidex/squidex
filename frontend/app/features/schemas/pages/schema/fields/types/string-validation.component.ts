/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    fadeAnimation,
    FieldDto,
    hasNoValue$,
    hasValue$,
    ModalModel,
    PatternDto,
    ResourceOwner,
    RootFieldDto,
    StringFieldPropertiesDto,
    Types,
    value$
} from '@app/shared';

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
    public editForm: FormGroup;

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
            this.editForm.setControl('isUnique',
                new FormControl(this.properties.isUnique));
        }

        this.editForm.setControl('maxLength',
            new FormControl(this.properties.maxLength));

        this.editForm.setControl('minLength',
            new FormControl(this.properties.minLength));

        this.editForm.setControl('pattern',
            new FormControl(this.properties.pattern));

        this.editForm.setControl('patternMessage',
            new FormControl(this.properties.patternMessage));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.showDefaultValue =
            hasNoValue$(this.editForm.controls['isRequired']);

        this.showPatternSuggestions =
            hasNoValue$(this.editForm.controls['pattern']);

        this.showPatternSuggestions =
            hasNoValue$(this.editForm.controls['pattern']);

        this.showPatternMessage =
            hasValue$(this.editForm.controls['pattern']);

        this.own(
            value$(this.editForm.controls['pattern'])
                .subscribe((value: string) => {
                    if (!value) {
                        this.editForm.controls['patternMessage'].setValue(undefined);
                    }

                    this.setPatternName();
                }));

        this.setPatternName();
    }

    public ngOnChanges() {
        this.setPatternName();
    }

    public setPattern(pattern: PatternDto) {
        this.editForm.controls['pattern'].setValue(pattern.pattern);
        this.editForm.controls['patternMessage'].setValue(pattern.message);
    }

    private setPatternName() {
        const value = this.editForm.controls['pattern'].value;

        if (!value) {
            this.patternName = '';
        } else {
            const matchingPattern = this.patterns.find(x => x.pattern === this.editForm.controls['pattern'].value);

            if (matchingPattern) {
                this.patternName = matchingPattern.name;
            } else {
                this.patternName = 'Advanced';
            }
        }
    }
}