/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-prerender-action',
    styleUrls: ['./prerender-action.component.scss'],
    templateUrl: './prerender-action.component.html'
})
export class PrerenderActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('token',
            new FormControl(this.action.token || '', [
                Validators.required
            ]));

        this.actionForm.setControl('url',
            new FormControl(this.action.url || '', [
                Validators.required
            ]));
    }
}