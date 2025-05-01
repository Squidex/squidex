/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BranchItem, CodeEditorComponent, FlowExecutionStateDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { StateStepComponent } from './state-step.component';

@Component({
    standalone: true,
    selector: 'sqx-state-details',
    styleUrls: ['./state-details.component.scss'],
    templateUrl: './state-details.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        StateStepComponent,
        TranslatePipe,
    ],
})
export class StateDetailsComponent {
    @Input({ required: true })
    public availableSteps: Record<string, RuleElementDto> = {};

    @Input({ required: true })
    public stepItems: BranchItem[] = [];

    @Input({ required: true })
    public state!: FlowExecutionStateDto;

    public isExpanded = false;

    public toggle() {
        this.isExpanded = !this.isExpanded;
    }
}