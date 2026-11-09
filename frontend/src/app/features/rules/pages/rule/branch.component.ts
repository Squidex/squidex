/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, input, output } from '@angular/core';
import { BranchItem, BranchList, FlowView, RuleElementDto, TranslatePipe } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { FlowStepAdd, FlowStepRemove, FlowStepUpdate } from './types';

@Component({
    selector: 'sqx-branch',
    styleUrls: ['./branch.component.scss'],
    templateUrl: './branch.component.html',
    imports: [
        RuleElementComponent,
        TranslatePipe,
    ],
})
export class BranchComponent {
    /** The available steps by type, to resolve the metadata of each step. */
    public readonly availableSteps = input.required<{ [name: string]: RuleElementDto }>();

    /** Allows the user to add, edit and remove steps. */
    public readonly isEditable = input.required({ transform: booleanAttribute });

    /** The whole flow, to resolve the nested branches. */
    public readonly flow = input.required<FlowView>();

    /** The ID of the step that owns this branch, or null for the root branch. */
    public readonly parentId = input.required<string | null | undefined>();

    /** The index of this branch within the parent step. */
    public readonly parentBranch = input.required<number>();

    /** The steps in this branch. */
    public readonly branchItems = input.required<BranchList>();

    /** The label above the branch, for example the condition. */
    public readonly branchLabel = input<string | undefined>('');

    /** The title above the branch. */
    public readonly branchTitle = input<string | undefined>('');

    /** Emits when the user wants to add a step. */
    public readonly stepAdd = output<FlowStepAdd>();

    /** Emits when the user wants to edit a step. */
    public readonly stepUpdate = output<FlowStepUpdate>();

    /** Emits when the user removes a step. */
    public readonly stepRemove = output<FlowStepRemove>();

    protected edit(item: BranchItem) {
        this.stepUpdate.emit({ id: item.id, values: item.step });
    }

    protected remove(item: BranchItem) {
        this.stepRemove.emit({ id: item.id, parentId: this.parentId(), branchIndex: this.parentBranch() });
    }

    protected add(index = 0) {
        const afterId = this.branchItems()[index]?.id;
        this.stepAdd.emit({ afterId, parentId: this.parentId(), branchIndex: this.parentBranch() });
    }
}
