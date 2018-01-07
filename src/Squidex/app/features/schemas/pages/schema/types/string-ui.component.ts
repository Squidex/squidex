/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-string-ui',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html'
})
export class StringUIComponent implements OnDestroy, OnInit {
    private editorSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public hideAllowedValues: Observable<boolean>;

    public ngOnDestroy() {
        this.editorSubscription.unsubscribe();
    }

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
            this.editForm.controls['editor'].valueChanges
                .startWith(this.properties.editor)
                .map(x => !x || x === 'Input' || x === 'TextArea' || x === 'RichText' || x === 'Markdown');

        this.editorSubscription =
            this.hideAllowedValues
                .subscribe(isSelection => {
                    if (isSelection) {
                        this.editForm.controls['allowedValues'].setValue(undefined);
                    }
                });
    }
}