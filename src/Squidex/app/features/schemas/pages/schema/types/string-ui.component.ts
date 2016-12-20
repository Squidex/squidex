/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { fadeAnimation, StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-string-ui',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html',
    animations: [
        fadeAnimation
    ]
})
export class StringUIComponent implements OnDestroy, OnInit {
    private editorSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('placeholder',
            new FormControl(this.properties.placeholder, [
                Validators.maxLength(100)
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues));

        this.hideAllowedValues =
            Observable.of(this.properties.editor)
                .merge(this.editForm.get('editor').valueChanges)
                .map(x => !x || x === 'Input' || x === 'TextArea');

        this.editorSubscription =
            this.hideAllowedValues.subscribe(isSelection => {
                if (isSelection) {
                    this.editForm.get('allowedValues').setValue(undefined);
                }
            });
    }

    public ngOnDestroy() {
        this.editorSubscription.unsubscribe();
    }
}