/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

@Component({
    selector: 'sqx-asset-changed-trigger',
    styleUrls: ['./asset-changed-trigger.component.scss'],
    templateUrl: './asset-changed-trigger.component.html'
})
export class AssetChangedTriggerComponent implements OnInit {
    @Input()
    public trigger: any;

    @Output()
    public triggerChanged = new EventEmitter<object>();

    public triggerFormSubmitted = false;
    public triggerForm =
        this.formBuilder.group({
            sendCreate: false,
            sendUpdate: false,
            sendRename: false,
            sendDelete: false
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.trigger = Object.assign({}, {
            sendCreate: false,
            sendUpdate: false,
            sendRename: false,
            sendDelete: false
        }, this.trigger || {});

        this.triggerFormSubmitted = false;
        this.triggerForm.reset();
        this.triggerForm.setValue(this.trigger);
    }

    public save() {
        this.triggerFormSubmitted = true;

        if (this.triggerForm.valid) {
            const trigger = this.triggerForm.value;

            this.triggerChanged.emit(trigger);
        }
    }
}