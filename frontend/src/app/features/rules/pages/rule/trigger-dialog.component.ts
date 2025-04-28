/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ALL_TRIGGERS, KeysPipe, ModalDialogComponent, RuleTriggerDto, SchemasState, TranslatePipe, TriggerForm } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { AssetChangedTriggerComponent } from '../../shared/triggers/asset-changed-trigger.component';
import { CommentTriggerComponent } from '../../shared/triggers/comment-trigger.component';
import { ContentChangedTriggerComponent } from '../../shared/triggers/content-changed-trigger.component';
import { SchemaChangedTriggerComponent } from '../../shared/triggers/schema-changed-trigger.component';
import { UsageTriggerComponent } from '../../shared/triggers/usage-trigger.component';

@Component({
    standalone: true,
    selector: 'sqx-trigger-dialog',
    styleUrls: ['./trigger-dialog.component.scss'],
    templateUrl: './trigger-dialog.component.html',
    imports: [
        AssetChangedTriggerComponent,
        AsyncPipe,
        CommentTriggerComponent,
        ContentChangedTriggerComponent,
        KeysPipe,
        ModalDialogComponent,
        ReactiveFormsModule,
        RuleElementComponent,
        SchemaChangedTriggerComponent,
        TranslatePipe,
        UsageTriggerComponent,
    ],
})
export class TriggerDialogComponent implements OnInit {

    @Input({ required: true })
    public trigger?: RuleTriggerDto;

    @Input({ required: true, transform: booleanAttribute })
    public isEditable = true;

    @Output()
    public dialogClose = new EventEmitter();

    @Output()
    public dialogSaved = new EventEmitter<RuleTriggerDto>();
    
    public supportedTriggers = ALL_TRIGGERS;

    public currentTrigger?: TriggerForm;

    constructor(
        public readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        if (this.trigger) {
            this.selectTrigger(this.trigger.triggerType, this.trigger);
        }
    }

    public selectTrigger(type: string, values?: any) {
        if (this.currentTrigger?.triggerType !== type) {
            this.currentTrigger = new TriggerForm(type);
            this.currentTrigger.setEnabled(this.isEditable);
        }

        if (values) {
            this.currentTrigger?.load(values || {});
        }
    }

    public save() {
        if (!this.isEditable || !this.currentTrigger) {
            return;
        }

        const values = this.currentTrigger.submit();
        if (!values) {
            return;
        }

        this.dialogSaved.emit(values);
    }
}