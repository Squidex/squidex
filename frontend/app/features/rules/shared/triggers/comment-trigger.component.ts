/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
    selector: 'sqx-comment-trigger',
    styleUrls: ['./comment-trigger.component.scss'],
    templateUrl: './comment-trigger.component.html'
})
export class CommentTriggerComponent implements OnChanges {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['triggerForm']) {
            this.triggerForm.setControl('condition',
                new FormControl());
        }

        this.triggerForm.patchValue(this.trigger);
    }
}