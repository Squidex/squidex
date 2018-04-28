/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
    selector: 'sqx-asset-changed-trigger',
    styleUrls: ['./asset-changed-trigger.component.scss'],
    templateUrl: './asset-changed-trigger.component.html'
})
export class AssetChangedTriggerComponent implements OnInit {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    @Input()
    public triggerFormSubmitted = false;

    public ngOnInit() {
        this.triggerForm.setControl('sendCreate',
            new FormControl(this.trigger.sendCreate || false));

        this.triggerForm.setControl('sendUpdate',
            new FormControl(this.trigger.sendUpdate || false));

        this.triggerForm.setControl('sendRename',
            new FormControl(this.trigger.sendRename || false));

        this.triggerForm.setControl('sendDelete',
            new FormControl(this.trigger.sendDelete || false));
    }
}