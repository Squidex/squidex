/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import {
    AppPatternDto,
    FieldDto,
    ImmutableArray,
    ModalView,
    StringFieldPropertiesDto
} from '@app/shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html'
})
export class StringValidationComponent implements OnDestroy, OnInit {
    private patternSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: StringFieldPropertiesDto;

    @Input()
    public patterns: ImmutableArray<AppPatternDto>;

    public showDefaultValue: Observable<boolean>;
    public showPatternMessage: boolean;
    public showPatternSuggestions: Observable<boolean>;

    public patternName: string;
    public patternsModal = new ModalView(false, false);

    public ngOnDestroy() {
        this.patternSubscription.unsubscribe();
    }

    public ngOnInit() {
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
            this.editForm.controls['isRequired'].valueChanges
                .startWith(this.properties.isRequired)
                .map(x => !x);

        this.showPatternSuggestions =
            this.editForm.controls['pattern'].valueChanges
                .startWith('')
                .map(x => !x || x.trim().length === 0);

        this.showPatternMessage =
            this.editForm.controls['pattern'].value && this.editForm.controls['pattern'].value.trim().length > 0;

        this.patternSubscription =
            this.editForm.controls['pattern'].valueChanges
                .subscribe((value: string) => {
                    if (!value || value.length === 0) {
                        this.editForm.controls['patternMessage'].setValue(undefined);
                    }
                    this.setPatternName();
                });

        this.setPatternName();
    }

    public setPattern(pattern: AppPatternDto) {
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