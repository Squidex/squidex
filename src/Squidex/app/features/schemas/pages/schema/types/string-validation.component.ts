/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import {
    ModalView,
    StringFieldPropertiesDto,
    UIRegexSuggestionDto,
    UIService
} from 'shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html'
})
export class StringValidationComponent implements OnDestroy, OnInit {
    private patternSubscription: Subscription;
    private uiSettingsSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public showDefaultValue: Observable<boolean>;
    public showPatternMessage: Observable<boolean>;
    public showPatternSuggestions: Observable<boolean>;

    public regexSuggestions: UIRegexSuggestionDto[] = [];

    public regexSuggestionsModal = new ModalView(false, false);

    constructor(
        private readonly uiService: UIService
    ) {
    }

    public ngOnDestroy() {
        this.patternSubscription.unsubscribe();
        this.uiSettingsSubscription.unsubscribe();
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

        this.showPatternMessage =
            this.editForm.controls['pattern'].valueChanges
                .startWith('')
                .map(x => x && x.trim().length > 0);

        this.showPatternSuggestions =
            this.editForm.controls['pattern'].valueChanges
                .startWith('')
                .map(x => !x || x.trim().length === 0);

        this.uiSettingsSubscription =
            this.uiService.getSettings()
                .subscribe(settings => {
                    this.regexSuggestions = settings.regexSuggestions;
                });

        this.patternSubscription =
            this.editForm.controls['pattern'].valueChanges
                .subscribe((value: string) => {
                    if (!value || value.length === 0) {
                        this.editForm.controls['patternMessage'].setValue(undefined);
                    }
                });
    }

    public setPattern(pattern: string) {
        this.editForm.controls['pattern'].setValue(pattern);
    }
}