/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, effect, input, output, signal, untracked, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { EntriesPipe, FormErrorComponent, ManualRuleTriggerDto, ModalDialogComponent, RulesService, RuleTriggerDto, RuleTriggerMetadataDto, SchemasState, TranslatePipe, TriggerForm } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { AssetChangedTriggerComponent } from '../../shared/triggers/asset-changed-trigger.component';
import { CommentTriggerComponent } from '../../shared/triggers/comment-trigger.component';
import { ContentChangedTriggerComponent } from '../../shared/triggers/content-changed-trigger.component';
import { CronJobTriggerComponent } from '../../shared/triggers/cron-job-trigger.component';
import { SchemaChangedTriggerComponent } from '../../shared/triggers/schema-changed-trigger.component';
import { UsageTriggerComponent } from '../../shared/triggers/usage-trigger.component';

@Component({
    selector: 'sqx-trigger-dialog',
    styleUrls: ['./trigger-dialog.component.scss'],
    templateUrl: './trigger-dialog.component.html',
    imports: [
        AssetChangedTriggerComponent,
        CommentTriggerComponent,
        ContentChangedTriggerComponent,
        CronJobTriggerComponent,
        EntriesPipe,
        FormErrorComponent,
        ModalDialogComponent,
        ReactiveFormsModule,
        RuleElementComponent,
        SchemaChangedTriggerComponent,
        TranslatePipe,
        UsageTriggerComponent,
    ],
})
export class TriggerDialogComponent {
    private readonly rulesService = inject(RulesService);

    protected readonly schemasState = inject(SchemasState);

    /** The trigger to edit, or null to add a new trigger. */
    public readonly trigger = input.required<RuleTriggerDto | null | undefined>();

    /** Allows the user to change and save the trigger. */
    public readonly isEditable = input.required({ transform: booleanAttribute });

    /** The available triggers by type, which the user can choose from. */
    public readonly availableTriggers = input.required<Record<string, RuleTriggerMetadataDto>>();

    /** Emits when the dialog is closed without saving. */
    public readonly dialogClose = output();

    /** Emits the validated trigger when the user saves it or selects a trigger without properties. */
    public readonly dialogSave = output<RuleTriggerDto>();

    protected readonly currentTrigger = signal<TriggerForm | undefined>(undefined);

    constructor() {
        effect(() => {
            const trigger = this.trigger();
            if (!trigger) {
                return;
            }

            untracked(() => {
                this.selectTrigger(trigger.triggerType, trigger);
            });
        });
    }

    protected selectTrigger(type: string, values?: any) {
        const metadata = this.availableTriggers()[type];

        if (!metadata) {
            return;
        }

        if (!metadata.hasProperties) {
            this.dialogSave.emit(new ManualRuleTriggerDto());
            return;
        }

        if (this.currentTrigger()?.triggerType !== type) {
            const currentTrigger = new TriggerForm(type);
            currentTrigger.setEnabled(this.isEditable());

            this.currentTrigger.set(currentTrigger);
        }

        if (values) {
            this.currentTrigger()?.load(values || {});
        }
    }

    protected save() {
        const currentTrigger = this.currentTrigger();
        if (!this.isEditable() || !currentTrigger) {
            return;
        }

        const values = currentTrigger.submit();
        if (!values) {
            return;
        }

        this.rulesService.validateTrigger(this.schemasState.appName, values)
            .subscribe({
                error: error =>{
                    currentTrigger.submitFailed(error);
                },
                complete: () => {
                    this.dialogSave.emit(values);
                },
            });
    }
}
