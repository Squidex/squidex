/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { fadeAnimation, FloatConverter } from 'shared';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html',
    animations: [
        fadeAnimation
    ]
})
export class NumberUIComponent implements OnDestroy, OnInit {
    private editorSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('editor',
            new FormControl('Input', [
                Validators.required
            ]));
        this.editForm.addControl('placeholder',
            new FormControl('', [
                Validators.maxLength(100)
            ]));
        this.editForm.addControl('allowedValues',
            new FormControl(undefined, []));

        this.hideAllowedValues =
            Observable.of(false)
                .merge(this.editForm.get('editor').valueChanges)
                .map(x => !x || x === 'Input' || x === 'Textarea');

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