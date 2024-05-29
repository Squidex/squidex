/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { debounceTime, Subscription } from 'rxjs';
import { ActionForm, ALL_TRIGGERS, ConfirmClickDirective, FormAlertComponent, KeysPipe, LayoutComponent, ListViewComponent, MessageBus, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState, SidebarMenuDirective, Subscriptions, TitleComponent, ToggleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe, TriggerForm, TriggerType, value$ } from '@app/shared';
import { GenericActionComponent } from '../../shared/actions/generic-action.component';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { AssetChangedTriggerComponent } from '../../shared/triggers/asset-changed-trigger.component';
import { CommentTriggerComponent } from '../../shared/triggers/comment-trigger.component';
import { ContentChangedTriggerComponent } from '../../shared/triggers/content-changed-trigger.component';
import { SchemaChangedTriggerComponent } from '../../shared/triggers/schema-changed-trigger.component';
import { UsageTriggerComponent } from '../../shared/triggers/usage-trigger.component';
import { RuleConfigured } from '../messages';

@Component({
    standalone: true,
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html',
    imports: [
        AssetChangedTriggerComponent,
        AsyncPipe,
        CommentTriggerComponent,
        ConfirmClickDirective,
        ContentChangedTriggerComponent,
        FormAlertComponent,
        FormsModule,
        GenericActionComponent,
        KeysPipe,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        RuleElementComponent,
        SchemaChangedTriggerComponent,
        SidebarMenuDirective,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
        UsageTriggerComponent,
    ],
})
export class RulePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();
    private currentTriggerSubscription?: Subscription;
    private currentActionSubscription?: Subscription;

    public supportedTriggers = ALL_TRIGGERS;
    public supportedActions: { [name: string]: RuleElementDto } = {};

    public rule?: RuleDto | null;

    public currentTrigger?: TriggerForm;
    public currentAction?: ActionForm;

    public isEnabled = false;
    public isEditable = false;

    public get isManual() {
        return this.rule?.triggerType === 'Manual';
    }

    public get actionElement() {
        return this.supportedActions![this.currentAction?.actionType || ''];
    }

    public get triggerElement() {
        return this.supportedTriggers[(this.currentTrigger?.triggerType || '') as TriggerType];
    }

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;

                this.initFromRule();
            });

        this.subscriptions.add(
            this.rulesState.selectedRule
                .subscribe(rule => {
                    this.rule = rule;

                    this.initFromRule();
                }));

        this.schemasState.loadIfNotLoaded();
    }

    private initFromRule() {
        if (this.rule && this.supportedActions) {
            this.isEditable = this.rule.canUpdate;
            this.isEnabled = this.rule.isEnabled;

            this.selectAction(this.rule.actionType, this.rule.action);
            this.selectTrigger(this.rule.triggerType, this.rule.trigger);
        } else {
            this.isEditable = true;
            this.isEnabled = false;

            this.resetAction();
            this.resetTrigger();
        }
    }

    public selectAction(type: string, values?: any) {
        const definition = this.supportedActions[type];

        if (this.currentAction?.actionType !== type && definition) {
            this.currentAction = new ActionForm(definition, type);
            this.currentAction.setEnabled(this.isEditable);
            this.currentActionSubscription?.unsubscribe();
            this.currentActionSubscription = this.subscribe(this.currentAction.form);
        }

        if (values) {
            this.currentAction?.load(values);
        }
    }

    public selectTrigger(type: TriggerType, values?: any) {
        if (this.currentTrigger?.triggerType !== type) {
            this.currentTrigger = new TriggerForm(type);
            this.currentTrigger.setEnabled(this.isEditable);
            this.currentTriggerSubscription?.unsubscribe();
            this.currentTriggerSubscription = this.subscribe(this.currentTrigger.form);
        }

        if (values) {
            this.currentTrigger?.load(values || {});
        }
    }

    private subscribe(form: AbstractControl) {
        return value$(form).pipe(debounceTime(100)).subscribe(() => this.publishState());
    }

    public resetAction() {
        this.currentAction = undefined;
    }

    public resetTrigger() {
        this.currentTrigger = undefined;
    }

    public trigger() {
        this.rulesState.trigger(this.rule!);
    }

    public save() {
        if (!this.isEditable || !this.currentAction || !this.currentTrigger) {
            return;
        }

        const action = this.currentAction.submit();

        if (!action) {
            return;
        }

        const trigger = this.currentTrigger.submit();

        if (!trigger || !action) {
            return;
        }

        const request: any = { trigger, action, isEnabled: this.isEnabled };

        if (this.rule) {
            this.rulesState.update(this.rule, request)
                .subscribe({
                    next: () => {
                        this.submitCompleted();
                    },
                    error: error => {
                        this.submitFailed(error);
                    },
                });
        } else {
            this.rulesState.create(request)
                .subscribe({
                    next: rule => {
                        this.submitCompleted();

                        this.router.navigate([rule.id], { relativeTo: this.route.parent, replaceUrl: true });
                    },
                    error: error => {
                        this.submitFailed(error);
                    },
                });
        }
    }

    private publishState() {
        if (!this.currentAction || !this.currentTrigger) {
            return;
        }

        if (!this.currentAction.form.valid || !this.currentTrigger.form.valid) {
            return;
        }

        this.messageBus.emit(new RuleConfigured(
            this.currentTrigger.getValue(),
            this.currentAction.getValue()));
    }

    private submitCompleted() {
        this.currentAction?.submitCompleted({ noReset: true });
        this.currentTrigger?.submitCompleted({ noReset: true });
    }

    private submitFailed(error: any) {
        this.currentAction?.submitFailed(error);
        this.currentTrigger?.submitFailed(error);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
