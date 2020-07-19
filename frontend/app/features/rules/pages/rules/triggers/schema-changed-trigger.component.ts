/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
    selector: 'sqx-schema-changed-trigger',
    styleUrls: ['./schema-changed-trigger.component.scss'],
    templateUrl: './schema-changed-trigger.component.html'
})
export class SchemaChangedTriggerComponent implements OnInit {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    public ngOnInit() {
        this.triggerForm.setControl('condition',
            new FormControl(this.trigger.condition || ''));
    }
}