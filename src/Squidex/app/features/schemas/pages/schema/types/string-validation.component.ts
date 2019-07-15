/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    fadeAnimation,
    FieldDto,
    hasNoValue$,
    ImmutableArray,
    ModalModel,
    PatternDto,
    ResourceOwner,
    RootFieldDto,
    StringFieldPropertiesDto,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    animations: [
        fadeAnimation
    ]
})
export class StringValidationComponent extends ResourceOwner implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: StringFieldPropertiesDto;

    @Input()
    public patterns: ImmutableArray<PatternDto>;

    public showDefaultValue: Observable<boolean>;
    public showPatternMessage: boolean;
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

        this.showPatternMessage =
            this.editForm.controls['pattern'].value && this.editForm.controls['pattern'].value.trim().length > 0;

        this.own(
            this.editForm.controls['pattern'].valueChanges
                .subscribe((value: string) => {
                    if (!value || value.length === 0) {
                        this.editForm.controls['patternMessage'].setValue(undefined);
                    }

                    this.setPatternName();
                }));

        this.setPatternName();
    }

    public setPattern(pattern: PatternDto) {
        this.patternName = pattern.name;
        this.editForm.controls['pattern'].setValue(pattern.pattern);
        this.editForm.controls['patternMessage'].setValue(pattern.message);
        this.showPatternMessage = true;
    }

    private setPatternName() {
        const matchingPattern = this.patterns.find(x => x.pattern === this.editForm.controls['pattern'].value);

        if (matchingPattern) {
            this.patternName = matchingPattern.name;
        } else if (this.editForm.controls['pattern'].value && this.editForm.controls['pattern'].value.trim() !== '') {
            this.patternName = 'Advanced';
        } else {
            this.patternName = '';
        }
    }
}