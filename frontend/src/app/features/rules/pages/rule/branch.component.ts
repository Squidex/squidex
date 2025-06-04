/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { BranchItem, BranchList, FlowView, RuleElementDto, TranslatePipe } from '@app/shared';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { FlowStepAdd, FlowStepRemove, FlowStepUpdate } from './types';

@Component({
    selector: 'sqx-branch',
    styleUrls: ['./branch.component.scss'],
    templateUrl: './branch.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RuleElementComponent,
        TranslatePipe,
    ]
})
export class BranchComponent {
    @Input({ required: true })
    public availableSteps: { [name: string]: RuleElementDto } = {};

    @Input({ required: true, transform: booleanAttribute })
    public isEditable = true;

    @Input({ required: true })
    public flow!: FlowView;

    @Input({ required: true })
    public parentId?: string | null;

    @Input({ required: true })
    public parentBranch = 0;

    @Input({ required: true })
    public branchItems: BranchList = [];

    @Input()
    public branchLabel?: string = '';

    @Input()
    public branchTitle?: string = '';

    @Output()
    public stepAdd = new EventEmitter<FlowStepAdd>();

    @Output()
    public stepUpdate = new EventEmitter<FlowStepUpdate>();

    @Output()
    public stepRemove = new EventEmitter<FlowStepRemove>();

    public edit(item: BranchItem) {
        this.stepUpdate.emit({ id: item.id, values: item.step });
    }

    public remove(item: BranchItem) {
        this.stepRemove.emit({ id: item.id, parentId: this.parentId, branchIndex: this.parentBranch });
    }

    public add(index = 0) {
        const afterId = this.branchItems[index]?.id;
        this.stepAdd.emit({ afterId, parentId: this.parentId, branchIndex: this.parentBranch });
    }
}