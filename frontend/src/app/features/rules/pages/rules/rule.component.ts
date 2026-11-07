/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ConfirmClickDirective, DropdownMenuComponent, DynamicRuleDto, DynamicUpdateRuleDto, EditableTitleComponent, FlowView, ModalDirective, ModalModel, ModalPlacementDirective, RuleElementDto, RulesState, RuleTriggerMetadataDto, ToggleComponent, TranslatePipe } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';

@Component({
    selector: 'sqx-rule',
    styleUrls: ['./rule.component.scss'],
    templateUrl: './rule.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        DropdownMenuComponent,
        EditableTitleComponent,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        RuleElementComponent,
        ToggleComponent,
        TranslatePipe,
    ],
})
export class RuleComponent {
    /** The available triggers by type, to resolve the metadata of the trigger. */
    public readonly availableTriggers = input.required<Record<string, RuleTriggerMetadataDto>>();

    /** The available steps by type, to resolve the metadata of each step. */
    public readonly availableSteps = input.required<Record<string, RuleElementDto>>();

    /** The rule to show. */
    public readonly rule = input.required<DynamicRuleDto>();

    public readonly flow = computed(() => {
        return new FlowView(this.rule().flow).getAllItems();
    });

    public readonly dropdown = new ModalModel();

    constructor(
        private readonly rulesState: RulesState,
    ) {
    }

    public delete() {
        this.rulesState.delete(this.rule());
    }

    public run() {
        this.rulesState.run(this.rule());
    }

    public runFromSnapshots() {
        this.rulesState.runFromSnapshots(this.rule());
    }

    public rename(name: string) {
        this.rulesState.update(this.rule(), new DynamicUpdateRuleDto({ name }));
    }

    public disable() {
        this.rulesState.update(this.rule(), new DynamicUpdateRuleDto({ isEnabled: false }));
    }

    public enable() {
        this.rulesState.update(this.rule(), new DynamicUpdateRuleDto({ isEnabled: true }));
    }

    public toggle() {
        const rule = this.rule();

        this.rulesState.update(rule, new DynamicUpdateRuleDto({ isEnabled: !rule.isEnabled }));
    }

    public trigger() {
        this.rulesState.trigger(this.rule());
    }
}
